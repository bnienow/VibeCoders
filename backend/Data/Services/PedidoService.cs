using System.Globalization;
using Backend.Domain.DTOs.Pedidos;
using Backend.Domain.Exceptions;
using Backend.Domain.Helpers;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.Services;
using Backend.Domain.Mappings;
using Backend.Domain.Models;
using Backend.Domain.Models.Enums;

namespace Backend.Data.Services;

/// <summary>
/// Tudo é gravado num único SalvarAsync, que o EF executa numa transação: ou grava tudo, ou nada.
/// Se outra compra mudar o estoque ou o saldo ao mesmo tempo, o Program.cs responde 409 (tente de novo).
/// </summary>
public class PedidoService : IPedidoService
{
    private const decimal TetoFiado = -250m;
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sem 0/O e 1/I, que confundem

    private readonly IPedidoRepository _pedidos;
    private readonly IItemRepository _itens;
    private readonly IIntervaloRepository _intervalos;
    private readonly IAlunoRepository _alunos;
    private readonly IContaRepository _contas;
    private readonly IContaService _conta;
    private readonly IUnitOfWork _uow;

    public PedidoService(IPedidoRepository pedidos, IItemRepository itens, IIntervaloRepository intervalos,
        IAlunoRepository alunos, IContaRepository contas, IContaService conta, IUnitOfWork uow)
    {
        _pedidos = pedidos;
        _itens = itens;
        _intervalos = intervalos;
        _alunos = alunos;
        _contas = contas;
        _conta = conta;
        _uow = uow;
    }

    public async Task<PedidoDto> CriarAntecipado(int usuarioId, CriarPedidoDto dto)
    {
        var intervalo = await _intervalos.BuscarPorId(dto.IntervaloId)
            ?? throw new RegraException("Intervalo não encontrado");

        if (DateTime.Now >= intervalo.FechamentoEm(dto.Data))
            throw new RegraException("Pedidos para este intervalo já fecharam");

        if (await _pedidos.TemAntecipado(usuarioId, dto.Data, dto.IntervaloId))
            throw new RegraException("Você já tem um pedido neste intervalo. Altere o pedido existente.");

        var itens = await ReservarItens(dto.Itens, dto.Data, dto.IntervaloId);
        var total = itens.Sum(i => i.Item.PrecoUnitario * i.Quantidade);

        await ValidarLimites(usuarioId, dto.Data, total, total);

        var pedido = await NovoPedido(usuarioId, dto.Data, intervalo, TipoVenda.Antecipado, FormaPagamento.Conta, itens);
        pedido.Status = StatusPedido.Aberto;

        await Movimentar(usuarioId, -total, TipoMovimento.Compra, pedido);
        await _uow.SalvarAsync();

        return await BuscarDto(pedido.Id);
    }

    public async Task<PedidoDto> VenderNoBalcao(VendaBalcaoDto dto)
    {
        var pedido = await Vender(dto);

        return await BuscarDto(pedido.Id);
    }

    /// <summary>
    /// Se a conta estourou o limite enquanto estava offline, a venda entra como à vista e volta sinalizada para conferência.
    /// </summary>
    public async Task<List<ResultadoSincronizacaoDto>> Sincronizar(List<VendaBalcaoDto> vendas)
    {
        var resultados = new List<ResultadoSincronizacaoDto>();

        for (var i = 0; i < vendas.Count; i++)
        {
            var venda = vendas[i];
            var (pedido, motivo) = await TentarVender(venda);
            var situacao = pedido != null ? "Aceita" : "Rejeitada";

            if (pedido == null && venda.FormaPagamento == FormaPagamento.Conta)
            {
                venda.FormaPagamento = FormaPagamento.AVista;
                (pedido, _) = await TentarVender(venda);
                if (pedido != null)
                    situacao = "ConvertidaParaAVista";
            }

            resultados.Add(new ResultadoSincronizacaoDto { Indice = i, Situacao = situacao, PedidoId = pedido?.Id, Motivo = motivo });
        }

        return resultados;
    }

    public async Task<List<PedidoDto>> DoUsuario(int usuarioId) =>
        (await _pedidos.DoUsuario(usuarioId)).Select(p => p.ParaDto()).ToList();

    public async Task<PedidoDto?> Detalhe(int id, int usuarioId, bool ehAdmin)
    {
        var pedido = await _pedidos.BuscarComDetalhes(id);

        if (pedido == null || (pedido.UsuarioId != usuarioId && !ehAdmin))
            return null;

        return pedido.ParaDto();
    }

    /// <summary>
    /// Devolve o estoque antigo, reserva o novo e lança na conta só a diferença de valor.
    /// </summary>
    public async Task<PedidoDto> Alterar(int pedidoId, int usuarioId, AlterarPedidoDto dto)
    {
        var pedido = await BuscarEditavel(pedidoId, usuarioId);

        foreach (var ip in pedido.Itens)
            ip.Item.Estoque += ip.Quantidade;

        var itens = await ReservarItens(dto.Itens, pedido.Data, pedido.IntervaloId);
        var novoTotal = itens.Sum(i => i.Item.PrecoUnitario * i.Quantidade);
        var diferenca = novoTotal - pedido.Total;

        await ValidarLimites(usuarioId, pedido.Data, novoTotal, diferenca, ignorarPedidoId: pedido.Id);

        // Remove as linhas que saíram e atualiza/adiciona as que ficaram, com o preço de agora
        foreach (var linhaRemovida in pedido.Itens.Where(ip => !itens.Any(i => i.Item.Id == ip.ItemId)).ToList())
            pedido.Itens.Remove(linhaRemovida);

        foreach (var (item, quantidade) in itens)
        {
            var linha = pedido.Itens.FirstOrDefault(ip => ip.ItemId == item.Id);
            if (linha == null)
            {
                linha = new ItemPedido { Item = item };
                pedido.Itens.Add(linha);
            }

            linha.Quantidade = quantidade;
            linha.PrecoUnitario = item.PrecoUnitario;
            linha.Subtotal = item.PrecoUnitario * quantidade;
        }

        pedido.Total = novoTotal;
        pedido.TemAlertaAlergia = await TemAlertaAlergia(usuarioId, itens);

        if (diferenca > 0)
            await Movimentar(usuarioId, -diferenca, TipoMovimento.Compra, pedido);
        else if (diferenca < 0)
            await Movimentar(usuarioId, -diferenca, TipoMovimento.Estorno, pedido);

        await _uow.SalvarAsync();

        return await BuscarDto(pedido.Id);
    }

    public async Task Cancelar(int pedidoId, int usuarioId)
    {
        var pedido = await BuscarEditavel(pedidoId, usuarioId);

        foreach (var ip in pedido.Itens)
            ip.Item.Estoque += ip.Quantidade;

        pedido.Status = StatusPedido.Cancelado;
        pedido.CanceladoEm = DateTime.Now;

        await Movimentar(usuarioId, pedido.Total, TipoMovimento.Estorno, pedido);
        await _uow.SalvarAsync();
    }

    public async Task Entregar(int pedidoId)
    {
        var pedido = await _pedidos.BuscarPorId(pedidoId)
            ?? throw new RegraException("Pedido não encontrado");

        if (pedido.Status is StatusPedido.Entregue or StatusPedido.Cancelado)
            throw new RegraException($"Pedido já está {pedido.Status}");

        pedido.Status = StatusPedido.Entregue;
        pedido.EntregueEm = DateTime.Now;

        await _uow.SalvarAsync();
    }

    /// <summary>
    /// "O balcão nunca fecha": sem janela e sem DispCardapio. Na conta, valida limites; à vista, só baixa o estoque.
    /// </summary>
    private async Task<Pedido> Vender(VendaBalcaoDto dto)
    {
        if (dto.FormaPagamento == FormaPagamento.Online)
            throw new RegraException("No balcão o pagamento é na conta ou à vista");

        // Admin não tem conta; aluno e adulto têm
        if (!await _contas.Existe(dto.UsuarioId))
            throw new RegraException("Usuário não encontrado");

        var agora = DateTime.Now;
        var hoje = DateOnly.FromDateTime(agora);

        // Intervalo em andamento, se houver (fora dos intervalos fica nulo)
        var intervalo = await _intervalos.EmAndamento(TimeOnly.FromDateTime(agora));

        var itens = await ReservarItens(dto.Itens, hoje, null);
        var total = itens.Sum(i => i.Item.PrecoUnitario * i.Quantidade);

        var pedido = await NovoPedido(dto.UsuarioId, hoje, intervalo, TipoVenda.Balcao, dto.FormaPagamento, itens);
        pedido.Status = StatusPedido.Entregue;
        pedido.EntregueEm = agora;

        if (dto.FormaPagamento == FormaPagamento.Conta)
        {
            await ValidarLimites(dto.UsuarioId, hoje, total, total);
            await Movimentar(dto.UsuarioId, -total, TipoMovimento.Compra, pedido);
        }

        await _uow.SalvarAsync();

        return pedido;
    }

    // Tenta a venda; se uma regra barrar, descarta o que ficou pela metade na memória e devolve o motivo
    private async Task<(Pedido? Pedido, string? Motivo)> TentarVender(VendaBalcaoDto venda)
    {
        try
        {
            return (await Vender(venda), null);
        }
        catch (RegraException e)
        {
            _uow.DescartarAlteracoes();
            return (null, e.Message);
        }
    }

    /// <summary>
    /// Confere cada item (existe, ativo, oferecido no intervalo, estoque suficiente) e já baixa o estoque.
    /// intervaloId nulo = balcão, que não consulta o DispCardapio.
    /// </summary>
    private async Task<List<(Item Item, int Quantidade)>> ReservarItens(List<ItemQuantidadeDto> pedidos, DateOnly data, int? intervaloId)
    {
        // Mesmo item repetido no carrinho vira uma linha só
        var quantidades = pedidos
            .GroupBy(p => p.ItemId)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantidade));

        var itens = await _itens.BuscarPorIds(quantidades.Keys.ToList());

        var reservados = new List<(Item, int)>();

        foreach (var (itemId, quantidade) in quantidades)
        {
            if (!itens.TryGetValue(itemId, out var item) || !item.Ativo)
                throw new RegraException($"Item {itemId} não encontrado");

            if (intervaloId != null && !await _itens.OferecidoNoIntervalo(itemId, data, intervaloId.Value))
                throw new RegraException($"{item.Nome} não está no cardápio deste intervalo");

            if (item.Estoque < quantidade)
                throw new RegraException($"{item.Nome} esgotou");

            item.Estoque -= quantidade;
            reservados.Add((item, quantidade));
        }

        return reservados;
    }

    /// <summary>
    /// Limite diário (D2) e teto de fiado. Só valem para aluno; adulto não tem limite.
    /// totalDoPedido conta no gasto do dia; debito é o que sai da conta agora.
    /// </summary>
    private async Task ValidarLimites(int usuarioId, DateOnly data, decimal totalDoPedido, decimal debito, int? ignorarPedidoId = null)
    {
        var aluno = await _alunos.BuscarPorId(usuarioId);
        if (aluno == null)
            return;

        if (aluno.LimiteDiario != null)
        {
            var gastoDoDia = await _pedidos.GastoNaContaDoDia(usuarioId, data, ignorarPedidoId);

            if (gastoDoDia + totalDoPedido > aluno.LimiteDiario)
                throw new RegraException($"Limite diário de {Reais(aluno.LimiteDiario.Value)} atingido");
        }

        var conta = await _contas.BuscarPorUsuario(usuarioId)
            ?? throw new RegraException("Conta não encontrada");
        if (conta.Saldo - debito < TetoFiado)
            throw new RegraException($"Limite de {Reais(-TetoFiado)} atingido — somente à vista");
    }

    /// <summary>
    /// Monta o pedido com o preço de cada item congelado no momento da compra.
    /// </summary>
    private async Task<Pedido> NovoPedido(int usuarioId, DateOnly data, Intervalo? intervalo, TipoVenda tipo,
        FormaPagamento forma, List<(Item Item, int Quantidade)> itens)
    {
        var pedido = new Pedido
        {
            UsuarioId = usuarioId,
            Data = data,
            Intervalo = intervalo,
            TipoVenda = tipo,
            FormaPagamento = forma,
            Total = itens.Sum(i => i.Item.PrecoUnitario * i.Quantidade),
            CodigoRetirada = await NovoCodigo(data),
            TemAlertaAlergia = await TemAlertaAlergia(usuarioId, itens),
            CriadoEm = DateTime.Now,
            Itens = itens.Select(i => new ItemPedido
            {
                Item = i.Item,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.Item.PrecoUnitario,
                Subtotal = i.Item.PrecoUnitario * i.Quantidade,
            }).ToList(),
        };

        _pedidos.Adicionar(pedido);

        return pedido;
    }

    // Lança na conta com a descrição "Pedido A7K2: 2x Coxinha, 1x Suco"
    private Task Movimentar(int usuarioId, decimal valor, TipoMovimento tipo, Pedido pedido)
    {
        var itens = string.Join(", ", pedido.Itens.Select(ip => $"{ip.Quantidade}x {ip.Item.Nome}"));

        return _conta.Movimentar(usuarioId, valor, tipo, $"Pedido {pedido.CodigoRetirada}: {itens}", pedido);
    }

    /// <summary>
    /// Pedido do próprio usuário, ainda Aberto e com a janela aberta.
    /// </summary>
    private async Task<Pedido> BuscarEditavel(int pedidoId, int usuarioId)
    {
        var pedido = await _pedidos.BuscarParaEditar(pedidoId, usuarioId)
            ?? throw new RegraException("Pedido não encontrado");

        if (!pedido.PodeAlterar())
            throw new RegraException("Pedido não pode mais ser alterado: a janela deste intervalo já fechou");

        return pedido;
    }

    // Algum item tem alérgeno que o comprador (se for aluno) não pode comer
    private async Task<bool> TemAlertaAlergia(int usuarioId, List<(Item Item, int Quantidade)> itens)
    {
        var aluno = await _alunos.BuscarPorId(usuarioId);

        return aluno != null && itens.Any(i => Alergia.TemConflito(i.Item.Alergenos, aluno.RestricoesAlimentares));
    }

    // Código curto de retirada (ex.: A7K2), único no dia
    private async Task<string> NovoCodigo(DateOnly data)
    {
        string codigo;
        do
            codigo = new string(Random.Shared.GetItems(Alfabeto.ToCharArray(), 4));
        while (await _pedidos.CodigoEmUso(data, codigo));

        return codigo;
    }

    // Relê o pedido do banco já com os detalhes, para devolver depois de gravar
    private async Task<PedidoDto> BuscarDto(int id) =>
        (await _pedidos.BuscarComDetalhes(id))!.ParaDto();

    // 250 → "R$ 250,00"
    private static string Reais(decimal valor) =>
        valor.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
}
