using Backend.DTOs.Relatorios;
using ClosedXML.Excel;

namespace Backend.Reports;

// Gera a planilha de vendas com ClosedXML: uma aba por dia, outra com o ranking de itens
public static class ExcelExportService
{
    public static byte[] Vendas(List<VendaDiaDto> dias, List<ItemVendidoDto> itens)
    {
        using var planilha = new XLWorkbook();

        planilha.Worksheets.Add("Vendas por dia").Cell(1, 1).InsertTable(dias);
        planilha.Worksheets.Add("Itens mais vendidos").Cell(1, 1).InsertTable(itens);

        using var arquivo = new MemoryStream();
        planilha.SaveAs(arquivo);

        return arquivo.ToArray();
    }
}
