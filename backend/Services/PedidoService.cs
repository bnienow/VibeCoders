using System.Globalization;
using Backend.Data;
using Backend.DTOs.Pedidos;
using Backend.Models;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// Regras de pedido (spec, seções 4 e 10): janela, estoque, limite diário, teto de fiado e lançamento na conta.
/// Tudo é gravado num único SaveChanges, que o EF executa numa transação: ou grava tudo, ou nada.
/// Se outra compra mudar o estoque ou o saldo ao mesmo tempo, o Program.cs responde 409 (tente de novo).
/// </summary>
public class PedidoService
{
    private const decimal TetoFiado = -250m;
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sem 0/O e 1/I, que confundem

    private readonly AppDbContext _db;
    private readonly ContaService _conta;

    public PedidoService(AppDbContext db, ContaService conta)
    {
        _db = db;
        _conta = conta;
    }

    /// <summary>
    /// Pedido antecipado: valida janela, disponibilidade, duplicidade e limites; debita a conta na hora.
    /// </summary>
    public async Task<Pedido> CriarAntecipado(int usuarioId, CriarPedidoDto dto)
    {
        var intervalo = await _db.Intervalos.FindAsync(dto.IntervaloId)
            ?? throw new RegraException("Intervalo não encontrado");

        if (DateTime.Now >= Fechamento(dto.Data, intervalo))
            throw new RegraException("Pedidos para este intervalo já fecharam");

        var jaTemPedido = await _db.Pedidos.AnyAsync(p =>
            p.UsuarioId == usuarioId &&
            p.Data == dto.Data &&
            p.IntervaloId == dto.IntervaloId &&
            p.TipoVenda == TipoVenda.Antecipado &&
            p.Status != StatusPedido.Cancelado);
        if (jaTemPedido)
            throw new RegraException("Você já tem um pedido neste intervalo. Altere o pedido existente.");

        var itens = await ReservarItens(dto.Itens, dto.Data, dto.IntervaloId);
        var total = itens.Sum(i => i.Item.PrecoUnitario * i.Quantidade);

        await ValidarLimites(usuarioId, dto.Data, total, total);

        var pedido = await NovoPedido(usuarioId, dto.Data, intervalo, TipoVenda.Antecipado, FormaPagamento.Conta, itens);
        pedido.Status = StatusPedido.Aberto;

        await Movimentar(usuarioId, -total, TipoMovimento.Compra, pedido);
        await _db.SaveChangesAsync();

        return pedido;
    }

    /// <summary>
    /// Venda no balcão: sem janela e sem DispCardapio ("o balcão nunca fecha"). Nasce Entregue.
    /// Na conta, valida limites; à vista, só baixa o estoque.
    /// </summary>
    public async Task<Pedido> VenderNoBalcao(VendaBalcaoDto dto)
    {
        if (dto.FormaPagamento == FormaPagamento.Online)
            throw new RegraException("No balcão o pagamento é na conta ou à vista");

        // Admin não tem conta; aluno e adulto têm
        var compradorExiste = await _db.Contas.AnyAsync(c => c.UsuarioId == dto.UsuarioId);
        if (!compradorExiste)
            throw new RegraException("Usuário não encontrado");

        var agora = DateTime.Now;
        var hoje = DateOnly.FromDateTime(agora);
        var hora = TimeOnly.FromDateTime(agora);

        // Intervalo em andamento, se houver (fora dos intervalos fica nulo)
        var intervalo = await _db.Intervalos.FirstOrDefaultAsync(i => i.HoraInicio <= hora && hora <= i.HoraFim);

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

        await _db.SaveChangesAsync();

        return pedido;
    }

    /// <summary>
    /// Vendas de balcão feitas sem internet (D6), reprocessadas com as regras normais.
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

    // Tenta a venda; se uma regra barrar, descarta o que ficou pela metade na memória e devolve o motivo
    private async Task<(Pedido? Pedido, string? Motivo)> TentarVender(VendaBalcaoDto venda)
    {
        try
        {
            return (await VenderNoBalcao(venda), null);
        }
        catch (RegraException e)
        {
            _db.ChangeTracker.Clear();
            return (null, e.Message);
        }
    }

    /// <summary>
    /// Troca os itens de um pedido Aberto: devolve o estoque antigo, reserva o novo
    /// e lança na conta só a diferença de valor.
    /// </summary>
    public async Task<Pedido> Alterar(int pedidoId, int usuarioId, AlterarPedidoDto dto)
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

        await _db.SaveChangesAsync();

        return pedido;
    }

    /// <summary>
    /// Cancela um pedido Aberto: devolve o estoque e estorna o valor na conta.
    /// </summary>
    public async Task Cancelar(int pedidoId, int usuarioId)
    {
        var pedido = await BuscarEditavel(pedidoId, usuarioId);

        foreach (var ip in pedido.Itens)
            ip.Item.Estoque += ip.Quantidade;

        pedido.Status = StatusPedido.Cancelado;
        pedido.CanceladoEm = DateTime.Now;

        await Movimentar(usuarioId, pedido.Total, TipoMovimento.Estorno, pedido);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Marca como entregue um pedido antecipado (painel da cantina).
    /// </summary>
    public async Task Entregar(int pedidoId)
    {
        var pedido = await _db.Pedidos.FindAsync(pedidoId)
            ?? throw new RegraException("Pedido não encontrado");

        if (pedido.Status is StatusPedido.Entregue or StatusPedido.Cancelado)
            throw new RegraException($"Pedido já está {pedido.Status}");

        pedido.Status = StatusPedido.Entregue;
        pedido.EntregueEm = DateTime.Now;

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Status para mostrar: um pedido Aberto cuja janela já fechou é Confirmado
    /// (a cozinha já está montando). Calculado na leitura, sem job rodando.
    /// </summary>
    public static StatusPedido StatusAtual(Pedido pedido) =>
        pedido.Status == StatusPedido.Aberto && !JanelaAberta(pedido)
            ? StatusPedido.Confirmado
            : pedido.Status;

    /// <summary>
    /// Pode alterar ou cancelar: pedido Aberto e janela ainda aberta.
    /// </summary>
    public static bool PodeAlterar(Pedido pedido) =>
        pedido.Status == StatusPedido.Aberto && JanelaAberta(pedido);

    // Precisa do Intervalo carregado
    private static bool JanelaAberta(Pedido pedido) =>
        pedido.Intervalo != null && DateTime.Now < Fechamento(pedido.Data, pedido.Intervalo);

    // Momento em que o intervalo para de aceitar pedido: dia + hora de início − antecedência (ex.: 08:45)
    private static DateTime Fechamento(DateOnly data, Intervalo intervalo) =>
        data.ToDateTime(intervalo.HoraInicio).AddMinutes(-intervalo.MinutosAntecedencia);

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

        var ids = quantidades.Keys.ToList();
        var itens = await _db.Itens
            .Where(i => ids.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id);

        var reservados = new List<(Item, int)>();

        foreach (var (itemId, quantidade) in quantidades)
        {
            if (!itens.TryGetValue(itemId, out var item) || !item.Ativo)
                throw new RegraException($"Item {itemId} não encontrado");

            if (intervaloId != null)
            {
                var oferecido = await _db.DispCardapios.AnyAsync(d =>
                    d.ItemId == itemId && d.Data == data && d.IntervaloId == intervaloId && d.Disponivel);
                if (!oferecido)
                    throw new RegraException($"{item.Nome} não está no cardápio deste intervalo");
            }

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
        var aluno = await _db.Alunos.FindAsync(usuarioId);
        if (aluno == null)
            return;

        if (aluno.LimiteDiario != null)
        {
            var gastoDoDia = await _db.Pedidos
                .Where(p => p.UsuarioId == usuarioId &&
                            p.Data == data &&
                            p.Id != ignorarPedidoId &&
                            p.FormaPagamento == FormaPagamento.Conta &&
                            p.Status != StatusPedido.Cancelado)
                .SumAsync(p => p.Total);

            if (gastoDoDia + totalDoPedido > aluno.LimiteDiario)
                throw new RegraException($"Limite diário de {Reais(aluno.LimiteDiario.Value)} atingido");
        }

        var conta = await _db.Contas.SingleAsync(c => c.UsuarioId == usuarioId);
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

        _db.Pedidos.Add(pedido);

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
        var pedido = await _db.Pedidos
            .Include(p => p.Intervalo)
            .Include(p => p.Itens).ThenInclude(ip => ip.Item)
            .FirstOrDefaultAsync(p => p.Id == pedidoId && p.UsuarioId == usuarioId)
            ?? throw new RegraException("Pedido não encontrado");

        if (!PodeAlterar(pedido))
            throw new RegraException("Pedido não pode mais ser alterado: a janela deste intervalo já fechou");

        return pedido;
    }

    // Algum item tem alérgeno que o comprador (se for aluno) não pode comer
    private async Task<bool> TemAlertaAlergia(int usuarioId, List<(Item Item, int Quantidade)> itens)
    {
        var aluno = await _db.Alunos.FindAsync(usuarioId);

        return aluno != null && itens.Any(i => Alergia.TemConflito(i.Item.Alergenos, aluno.RestricoesAlimentares));
    }

    // Código curto de retirada (ex.: A7K2), único no dia
    private async Task<string> NovoCodigo(DateOnly data)
    {
        string codigo;
        do
            codigo = new string(Random.Shared.GetItems(Alfabeto.ToCharArray(), 4));
        while (await _db.Pedidos.AnyAsync(p => p.Data == data && p.CodigoRetirada == codigo));

        return codigo;
    }


    // 250 → "R$ 250,00"
    private static string Reais(decimal valor) =>
        valor.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
}
