using Backend.DTOs.Relatorios;
using ClosedXML.Excel;

namespace Backend.Reports;

// Gera a planilha de vendas com ClosedXML: uma aba com as vendas por dia, outra com o ranking de itens.
// As duas começam com o período filtrado, para a planilha dizer sozinha de quando são os números.
public static class ExcelExportService
{
    private const string FormatoMoeda = "\"R$\" #,##0.00";

    public static byte[] Vendas(DateOnly de, DateOnly ate, List<VendaDiaDto> dias, List<ItemVendidoDto> itens)
    {
        using var planilha = new XLWorkbook();
        var periodo = $"Período: {de:dd/MM/yyyy} a {ate:dd/MM/yyyy}";

        var abaDias = NovaAba(planilha, "Vendas por dia", periodo);
        abaDias.Cell(3, 1).InsertTable(dias.Select(d => new
        {
            Data = d.Data.ToDateTime(TimeOnly.MinValue), // DateTime: o Excel reconhece como data (filtra e ordena)
            d.Pedidos,
            d.Total,
        }));
        abaDias.Column(1).Style.DateFormat.Format = "dd/mm/yyyy";
        abaDias.Column(3).Style.NumberFormat.Format = FormatoMoeda;
        abaDias.Columns().AdjustToContents();

        var abaItens = NovaAba(planilha, "Itens mais vendidos", periodo);
        abaItens.Cell(3, 1).InsertTable(itens);
        abaItens.Column(3).Style.NumberFormat.Format = FormatoMoeda;
        abaItens.Columns().AdjustToContents();

        using var arquivo = new MemoryStream();
        planilha.SaveAs(arquivo);

        return arquivo.ToArray();
    }

    // Aba com o período na linha 1 (a tabela começa na linha 3)
    private static IXLWorksheet NovaAba(XLWorkbook planilha, string nome, string periodo)
    {
        var aba = planilha.Worksheets.Add(nome);
        aba.Cell(1, 1).Value = periodo;
        aba.Cell(1, 1).Style.Font.Bold = true;
        return aba;
    }
}
