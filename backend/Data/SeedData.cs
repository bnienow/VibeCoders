using System.Globalization;
using System.Text;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

// Massa de teste fictícia (spec, seção 12). Só popula banco vazio.
// Nenhum dado real: nomes, e-mails, CPFs e telefones são inventados.
public static class SeedData
{
    private const string SenhaAdmin = "admin123";
    private const string SenhaPadrao = "senha123";
    private const string DominioAdulto = "email.test";
    private const int DiasDeCardapio = 7;

    // Semente fixa: a massa sai igual toda vez que o banco é recriado
    private static readonly Random Rnd = new(42);

    private record ItemSeed(string Nome, string Descricao, decimal Preco, CategoriaItem Categoria, string Alergenos);

    private record AlunoSeed(string Nome, string Turma, int Idade, int Responsavel, decimal SaldoFinal,
        decimal? LimiteDiario = null, string Restricoes = "");

    private static readonly ItemSeed[] ItensSeed =
    [
        new("Pão de queijo", "Porção com 3 unidades, assado na hora", 4.50m, CategoriaItem.Salgado, "Lactose"),
        new("Coxinha", "Coxinha de frango com catupiry", 7.00m, CategoriaItem.Salgado, "Gluten,Lactose"),
        new("Pastel de carne", "Pastel frito de carne moída", 7.50m, CategoriaItem.Salgado, "Gluten"),
        new("Pastel de queijo", "Pastel frito de queijo muçarela", 7.50m, CategoriaItem.Salgado, "Gluten,Lactose"),
        new("Esfiha de carne", "Esfiha aberta de carne temperada", 6.50m, CategoriaItem.Salgado, "Gluten"),
        new("Enroladinho de salsicha", "Massa assada com salsicha", 6.00m, CategoriaItem.Salgado, "Gluten"),
        new("Empada de frango", "Empada de massa podre com frango", 7.00m, CategoriaItem.Salgado, "Gluten,Lactose,Ovo"),
        new("Quibe", "Quibe frito de carne e trigo", 6.50m, CategoriaItem.Salgado, "Gluten"),
        new("Misto quente", "Pão de forma com presunto e queijo na chapa", 8.00m, CategoriaItem.Salgado, "Gluten,Lactose"),
        new("Sanduíche natural", "Pão integral com frango, cenoura e requeijão", 9.50m, CategoriaItem.Salgado, "Gluten,Lactose"),
        new("Pão de batata", "Pão de batata recheado com requeijão", 6.50m, CategoriaItem.Salgado, "Gluten,Lactose"),
        new("Tapioca de queijo", "Tapioca com queijo coalho", 8.50m, CategoriaItem.Salgado, "Lactose"),

        new("Brigadeiro", "Brigadeiro tradicional", 3.00m, CategoriaItem.Doce, "Lactose"),
        new("Bolo de cenoura", "Fatia com cobertura de chocolate", 5.50m, CategoriaItem.Doce, "Gluten,Lactose,Ovo"),
        new("Bolo de chocolate", "Fatia de bolo de chocolate", 5.50m, CategoriaItem.Doce, "Gluten,Lactose,Ovo"),
        new("Cookie", "Cookie com gotas de chocolate", 4.50m, CategoriaItem.Doce, "Gluten,Lactose,Ovo"),
        new("Paçoca", "Paçoca de amendoim", 2.00m, CategoriaItem.Doce, "Amendoim"),
        new("Pé de moleque", "Doce de amendoim com rapadura", 2.50m, CategoriaItem.Doce, "Amendoim"),
        new("Brownie", "Brownie de chocolate meio amargo", 6.00m, CategoriaItem.Doce, "Gluten,Lactose,Ovo"),
        new("Alfajor", "Alfajor com doce de leite", 5.00m, CategoriaItem.Doce, "Gluten,Lactose"),
        new("Salada de frutas", "Copo de 300 ml com frutas da estação", 7.00m, CategoriaItem.Doce, ""),
        new("Barra de cereal", "Barra de cereal com mel", 3.50m, CategoriaItem.Doce, "Gluten,Soja"),

        new("Água mineral", "Garrafa de 500 ml", 3.00m, CategoriaItem.Bebida, ""),
        new("Água com gás", "Garrafa de 500 ml", 3.50m, CategoriaItem.Bebida, ""),
        new("Suco de laranja", "Copo de 300 ml, natural", 6.00m, CategoriaItem.Bebida, ""),
        new("Suco de uva", "Copo de 300 ml, integral", 6.00m, CategoriaItem.Bebida, ""),
        new("Suco de maracujá", "Copo de 300 ml, natural", 6.00m, CategoriaItem.Bebida, ""),
        new("Refrigerante lata", "Lata de 350 ml", 5.50m, CategoriaItem.Bebida, ""),
        new("Chá gelado", "Garrafa de 300 ml, pêssego", 5.00m, CategoriaItem.Bebida, ""),
        new("Achocolatado", "Caixinha de 200 ml", 4.50m, CategoriaItem.Bebida, "Lactose,Soja"),
        new("Iogurte", "Iogurte de morango, 170 g", 5.00m, CategoriaItem.Bebida, "Lactose"),
        new("Vitamina de banana", "Copo de 300 ml, batida com leite", 7.50m, CategoriaItem.Bebida, "Lactose"),
        new("Leite", "Copo de 200 ml", 3.50m, CategoriaItem.Bebida, "Lactose"),
        new("Café com leite", "Copo de 200 ml", 4.00m, CategoriaItem.Bebida, "Lactose"),

        new("Combo Lanche", "Pão de queijo + suco de laranja", 9.50m, CategoriaItem.Combo, "Lactose"),
        new("Combo Coxinha", "Coxinha + refrigerante lata", 11.00m, CategoriaItem.Combo, "Gluten,Lactose"),
        new("Combo Misto", "Misto quente + achocolatado", 11.00m, CategoriaItem.Combo, "Gluten,Lactose,Soja"),
        new("Combo Doce", "Brigadeiro + água mineral", 5.50m, CategoriaItem.Combo, "Lactose"),
    ];

    private static readonly string[] Responsaveis =
    [
        "Carla Andrade", "Roberto Kunz", "Fernanda Schmitt", "Marcos Weber", "Patrícia Becker",
        "Eduardo Rocha", "Simone Lange", "Ricardo Hoffmann", "Luciana Fischer", "André Müller",
    ];

    private static readonly AlunoSeed[] AlunosSeed =
    [
        new("Lucas Andrade", "9A", 14, 0, 85.00m),                            // saldo positivo
        new("Marina Andrade", "6B", 11, 0, 40.00m),
        new("Pedro Kunz", "1EM", 15, 1, -240.00m),                            // cena 5: perto do teto
        new("Sofia Kunz", "7A", 12, 1, 20.00m),
        new("Gabriela Schmitt", "8B", 13, 2, 35.00m, Restricoes: "Lactose"),  // D3: restrição de lactose
        new("Rafael Schmitt", "2EM", 16, 2, 15.00m, LimiteDiario: 10.00m),    // D2: limite diário baixo
        new("Júlia Weber", "9B", 14, 3, -30.00m),
        new("Henrique Weber", "3EM", 17, 3, 60.00m),
        new("Laura Becker", "6A", 11, 4, 12.00m),
        new("Mateus Becker", "1EM", 15, 4, -80.00m),
        new("Isabela Rocha", "7B", 12, 5, 50.00m, Restricoes: "Amendoim"),
        new("Thiago Rocha", "2EM", 16, 5, 0.00m),
        new("Beatriz Lange", "8A", 13, 6, 25.00m),
        new("Enzo Lange", "3EM", 18, 6, -120.00m),
        new("Valentina Hoffmann", "9A", 14, 7, 70.00m),
        new("Bruno Hoffmann", "6B", 11, 7, 5.00m, LimiteDiario: 15.00m),
        new("Alice Fischer", "1EM", 15, 8, 30.00m, Restricoes: "Gluten"),
        new("Davi Fischer", "7A", 12, 8, -15.00m),
        new("Helena Müller", "2EM", 16, 9, 45.00m),
        new("Arthur Müller", "8B", 13, 9, 18.00m),
    ];

    // Pedidos antecipados de hoje, para o painel (cena 2) abrir populado
    private static readonly int[] PedidosHojeManha = [0, 4, 7, 10, 14];
    private static readonly int[] PedidosHojeTarde = [3, 6, 8, 12, 18];

    public static void Popular(AppDbContext db, IPasswordHasher<Usuario> hasher)
    {
        if (db.Usuarios.Any()) return;

        using var transacao = db.Database.BeginTransaction();

        var agora = DateTime.Now;
        var hoje = DateOnly.FromDateTime(agora);
        var codigosPorDia = new Dictionary<DateOnly, HashSet<string>>();

        db.Usuarios.Add(CriarUsuario(hasher, "Cantina", "admin@cantina.test", Permissao.Admin, SenhaAdmin, agora));

        var manha = new Intervalo { Nome = "Manhã", HoraInicio = new(9, 0), HoraFim = new(9, 20), MinutosAntecedencia = 15 };
        var tarde = new Intervalo { Nome = "Tarde", HoraInicio = new(15, 30), HoraFim = new(15, 50), MinutosAntecedencia = 15 };
        db.Intervalos.AddRange(manha, tarde);

        var itens = ItensSeed.Select(s => new Item
        {
            Nome = s.Nome, Descricao = s.Descricao, PrecoUnitario = s.Preco, Categoria = s.Categoria,
            Alergenos = s.Alergenos, Estoque = Rnd.Next(30, 81),
        }).ToList();
        itens.Single(i => i.Nome == "Alfajor").Estoque = 0; // esgotado: não aparece no cardápio
        db.Itens.AddRange(itens);

        db.DispCardapios.AddRange(CriarDisponibilidade(itens, manha, tarde, hoje));

        var adultos = Responsaveis.Select((nome, i) => CriarAdulto(hasher, nome, i, agora)).ToList();
        db.Adultos.AddRange(adultos);
        db.Contas.AddRange(adultos.Select(a => new Conta { Usuario = a.Usuario, Saldo = 0, AtualizadaEm = agora }));

        var diasLetivos = DiasLetivosDoMesAnterior(hoje);

        for (var i = 0; i < AlunosSeed.Length; i++)
        {
            var seed = AlunosSeed[i];
            var aluno = CriarAluno(hasher, seed, adultos[seed.Responsavel], hoje, agora);
            db.Alunos.Add(aluno);

            var pedidos = CriarHistorico(aluno, seed, diasLetivos, manha, tarde, itens, codigosPorDia);

            var intervaloHoje = PedidosHojeManha.Contains(i) ? manha : PedidosHojeTarde.Contains(i) ? tarde : null;
            if (intervaloHoje is not null)
                pedidos.Add(CriarPedidoDeHoje(aluno, hoje, intervaloHoje, itens, codigosPorDia));

            db.Pedidos.AddRange(pedidos);
            db.Contas.Add(CriarContaComMovimentos(aluno, seed.SaldoFinal, pedidos, diasLetivos[0], agora));
        }

        db.SaveChanges();

        // Adulto ↔ MetodoPagamento é circular (lista + padrão): o padrão só pode ser ligado depois do primeiro save
        foreach (var (adulto, i) in adultos.Select((a, i) => (a, i)))
        {
            var pix = new MetodoPagamento { Adulto = adulto, Tipo = TipoMetodoPagamento.Pix, Apelido = "Pix principal", UltimosDigitos = "" };
            db.MetodosPagamento.Add(pix);
            if (i % 2 == 0)
            {
                db.MetodosPagamento.Add(new MetodoPagamento
                {
                    Adulto = adulto, Tipo = TipoMetodoPagamento.Cartao, Apelido = "Cartão de crédito",
                    UltimosDigitos = Rnd.Next(1000, 10000).ToString(),
                });
            }
            adulto.MetodoPagamentoPadrao = pix;
        }

        db.SaveChanges();
        transacao.Commit();
    }

    private static Usuario CriarUsuario(IPasswordHasher<Usuario> hasher, string nome, string email,
        Permissao permissao, string senha, DateTime agora)
    {
        var usuario = new Usuario { Nome = nome, Email = email, Permissao = permissao, CriadoEm = agora };
        usuario.SenhaHash = hasher.HashPassword(usuario, senha);
        return usuario;
    }

    private static Adulto CriarAdulto(IPasswordHasher<Usuario> hasher, string nome, int indice, DateTime agora) => new()
    {
        Usuario = CriarUsuario(hasher, nome, $"{Slug(nome)}@{DominioAdulto}", Permissao.Adulto, SenhaPadrao, agora),
        Cpf = $"000.000.{indice + 1:000}-{indice + 10:00}",
        Telefone = $"(51) 90000-{indice + 1:0000}",
    };

    private static Aluno CriarAluno(IPasswordHasher<Usuario> hasher, AlunoSeed seed, Adulto responsavel,
        DateOnly hoje, DateTime agora) => new()
    {
        Usuario = CriarUsuario(hasher, seed.Nome, $"{Slug(seed.Nome)}@{Aluno.DominioEmail}", Permissao.Aluno, SenhaPadrao, agora),
        Adulto = responsavel,
        Turma = seed.Turma,
        DataNascimento = hoje.AddYears(-seed.Idade).AddDays(-Rnd.Next(0, 300)),
        LimiteDiario = seed.LimiteDiario,
        RestricoesAlimentares = seed.Restricoes,
    };

    // Hoje + próximos dias, para a demo funcionar mesmo depois que as janelas de hoje fecharem
    private static IEnumerable<DispCardapio> CriarDisponibilidade(List<Item> itens, Intervalo manha, Intervalo tarde, DateOnly hoje)
    {
        string[] soManha = ["Café com leite", "Pão de batata"];
        string[] soTarde = ["Salada de frutas"];

        for (var d = 0; d < DiasDeCardapio; d++)
        {
            var data = hoje.AddDays(d);
            foreach (var item in itens)
            {
                yield return new DispCardapio { Data = data, Intervalo = manha, Item = item, Disponivel = !soTarde.Contains(item.Nome) };
                yield return new DispCardapio { Data = data, Intervalo = tarde, Item = item, Disponivel = !soManha.Contains(item.Nome) };
            }
        }
    }

    private static List<DateOnly> DiasLetivosDoMesAnterior(DateOnly hoje)
    {
        var inicio = new DateOnly(hoje.Year, hoje.Month, 1).AddMonths(-1);
        return Enumerable.Range(0, DateTime.DaysInMonth(inicio.Year, inicio.Month))
            .Select(inicio.AddDays)
            .Where(d => d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            .ToList();
    }

    private static List<Pedido> CriarHistorico(Aluno aluno, AlunoSeed seed, List<DateOnly> dias,
        Intervalo manha, Intervalo tarde, List<Item> itens, Dictionary<DateOnly, HashSet<string>> codigos)
    {
        var pedidos = new List<Pedido>();
        var ocupados = new HashSet<(DateOnly, Intervalo)>();

        void TentarPedido(DateOnly dia, Intervalo intervalo)
        {
            if (!ocupados.Add((dia, intervalo))) return;
            var escolhidos = EscolherItens(itens, seed.Restricoes, seed.LimiteDiario);
            if (escolhidos.Count == 0) return;

            var balcao = Rnd.NextDouble() < 0.4;
            var forma = balcao && Rnd.NextDouble() < 0.25 ? FormaPagamento.AVista : FormaPagamento.Conta;
            var pedido = CriarPedido(aluno, dia, intervalo, balcao ? TipoVenda.Balcao : TipoVenda.Antecipado,
                forma, StatusPedido.Entregue, escolhidos, codigos);
            pedido.EntregueEm = dia.ToDateTime(intervalo.HoraInicio).AddMinutes(Rnd.Next(1, 15));
            pedidos.Add(pedido);
        }

        foreach (var dia in dias.Where(_ => Rnd.NextDouble() < 0.6))
            TentarPedido(dia, Rnd.Next(2) == 0 ? manha : tarde);

        // Saldo final negativo exige compras suficientes na conta para chegar nele
        while (seed.SaldoFinal + TotalNaConta(pedidos) < 0)
            TentarPedido(dias[Rnd.Next(dias.Count)], Rnd.Next(2) == 0 ? manha : tarde);

        return pedidos;
    }

    private static Pedido CriarPedidoDeHoje(Aluno aluno, DateOnly hoje, Intervalo intervalo, List<Item> itens,
        Dictionary<DateOnly, HashSet<string>> codigos)
    {
        // Aluna com restrição de lactose pede pão de queijo: pedido nasce com alerta de alergia (D3)
        var escolhidos = aluno.RestricoesAlimentares == "Lactose"
            ? new List<(Item, int)> { (itens.Single(i => i.Nome == "Pão de queijo"), 1), (itens.Single(i => i.Nome == "Suco de laranja"), 1) }
            : EscolherItens(itens, aluno.RestricoesAlimentares, aluno.LimiteDiario);

        foreach (var (item, quantidade) in escolhidos)
            item.Estoque -= quantidade; // estoque baixa na confirmação

        return CriarPedido(aluno, hoje, intervalo, TipoVenda.Antecipado, FormaPagamento.Conta,
            StatusPedido.Confirmado, escolhidos, codigos);
    }

    private static List<(Item Item, int Quantidade)> EscolherItens(List<Item> itens, string restricoes, decimal? limite)
    {
        var permitidos = itens.Where(i => i.Estoque > 0 && !Alergia.TemConflito(i.Alergenos, restricoes)).ToList();
        var escolhidos = permitidos.OrderBy(_ => Rnd.Next()).Take(Rnd.Next(1, 4))
            .Select(i => (Item: i, Quantidade: Rnd.NextDouble() < 0.8 ? 1 : 2))
            .ToList();

        while (limite is not null && escolhidos.Count > 0 && escolhidos.Sum(e => e.Item.PrecoUnitario * e.Quantidade) > limite)
            escolhidos.RemoveAt(escolhidos.Count - 1);

        return escolhidos;
    }

    private static Pedido CriarPedido(Aluno aluno, DateOnly dia, Intervalo intervalo, TipoVenda tipo,
        FormaPagamento forma, StatusPedido status, List<(Item Item, int Quantidade)> escolhidos,
        Dictionary<DateOnly, HashSet<string>> codigos)
    {
        var itensPedido = escolhidos.Select(e => new ItemPedido
        {
            Item = e.Item,
            Quantidade = e.Quantidade,
            PrecoUnitario = e.Item.PrecoUnitario,
            Subtotal = e.Item.PrecoUnitario * e.Quantidade,
        }).ToList();

        return new Pedido
        {
            Usuario = aluno.Usuario,
            Data = dia,
            Intervalo = intervalo,
            Status = status,
            TipoVenda = tipo,
            FormaPagamento = forma,
            Total = itensPedido.Sum(ip => ip.Subtotal),
            CodigoRetirada = NovoCodigo(dia, codigos),
            TemAlertaAlergia = escolhidos.Any(e => Alergia.TemConflito(e.Item.Alergenos, aluno.RestricoesAlimentares)),
            CriadoEm = dia.ToDateTime(intervalo.HoraInicio).AddMinutes(-Rnd.Next(20, 120)),
            Itens = itensPedido,
        };
    }

    // Crédito inicial do responsável + uma compra por pedido na conta; o saldo sai exatamente em saldoFinal
    private static Conta CriarContaComMovimentos(Aluno aluno, decimal saldoFinal, List<Pedido> pedidos,
        DateOnly primeiroDia, DateTime agora)
    {
        var conta = new Conta { Usuario = aluno.Usuario, AtualizadaEm = agora };
        var saldo = 0m;

        var credito = saldoFinal + TotalNaConta(pedidos);
        if (credito > 0)
        {
            saldo += credito;
            conta.Movimentos.Add(new Movimento
            {
                Tipo = TipoMovimento.Credito, Valor = credito, SaldoApos = saldo,
                Data = primeiroDia.ToDateTime(new TimeOnly(7, 0)), Descricao = "Crédito adicionado pelo responsável",
            });
        }

        foreach (var pedido in pedidos.Where(p => p.FormaPagamento == FormaPagamento.Conta).OrderBy(p => p.CriadoEm))
        {
            saldo -= pedido.Total;
            conta.Movimentos.Add(new Movimento
            {
                Tipo = TipoMovimento.Compra, Valor = -pedido.Total, SaldoApos = saldo, Data = pedido.CriadoEm,
                Pedido = pedido, Descricao = string.Join(", ", pedido.Itens.Select(ip => $"{ip.Quantidade}x {ip.Item.Nome}")),
            });
        }

        conta.Saldo = saldo;
        return conta;
    }

    private static decimal TotalNaConta(IEnumerable<Pedido> pedidos) =>
        pedidos.Where(p => p.FormaPagamento == FormaPagamento.Conta).Sum(p => p.Total);

    // Código curto sem caracteres ambíguos (0/O, 1/I), único por dia
    private static string NovoCodigo(DateOnly dia, Dictionary<DateOnly, HashSet<string>> codigos)
    {
        const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var usados = codigos.TryGetValue(dia, out var set) ? set : codigos[dia] = [];
        string codigo;
        do codigo = new string(Enumerable.Range(0, 4).Select(_ => alfabeto[Rnd.Next(alfabeto.Length)]).ToArray());
        while (!usados.Add(codigo));
        return codigo;
    }

    // "Júlia Weber" → "julia.weber"
    private static string Slug(string nome)
    {
        var semAcento = new string(nome.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        return semAcento.ToLowerInvariant().Replace(' ', '.');
    }
}
