using System.Globalization;
using Backend.DTOs.Extratos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Backend.Reports;

// Gera o PDF do extrato com QuestPDF
public static class PdfExportService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static byte[] Extrato(ExtratoDto extrato) =>
        Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4);
            pagina.Margin(30);
            pagina.DefaultTextStyle(t => t.FontSize(10));

            pagina.Header().Column(c =>
            {
                c.Item().Text($"Extrato — {extrato.AlunoNome}").FontSize(16).Bold();
                c.Item().Text($"{extrato.Mes.ToString("MMMM 'de' yyyy", PtBr)} · Saldo atual {extrato.SaldoAtual.ToString("C", PtBr)}");
                c.Item().Text($"Gasto no mês {extrato.TotalGasto.ToString("C", PtBr)} · Créditos {extrato.TotalCreditos.ToString("C", PtBr)}");
            });

            pagina.Content().PaddingTop(15).Table(tabela =>
            {
                tabela.ColumnsDefinition(colunas =>
                {
                    colunas.ConstantColumn(80);  // data
                    colunas.RelativeColumn();    // descrição
                    colunas.ConstantColumn(70);  // valor
                    colunas.ConstantColumn(70);  // saldo
                });

                tabela.Header(cabecalho =>
                {
                    cabecalho.Cell().Text("Data").Bold();
                    cabecalho.Cell().Text("Descrição").Bold();
                    cabecalho.Cell().AlignRight().Text("Valor").Bold();
                    cabecalho.Cell().AlignRight().Text("Saldo").Bold();
                });

                foreach (var m in extrato.Movimentos)
                {
                    tabela.Cell().PaddingVertical(2).Text(m.Data.ToString("dd/MM HH:mm"));
                    tabela.Cell().PaddingVertical(2).Text(m.Descricao);
                    tabela.Cell().PaddingVertical(2).AlignRight().Text(m.Valor.ToString("C", PtBr));
                    tabela.Cell().PaddingVertical(2).AlignRight().Text(m.SaldoApos.ToString("C", PtBr));
                }
            });
        })).GeneratePdf();
}
