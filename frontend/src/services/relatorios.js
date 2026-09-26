import { money } from './cantinaService';
const escapeXml = (s) => String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;').replaceAll('"','&quot;');
export function resumirVendas(pedidos, inicio, fim) {
  const rows = pedidos.filter((p) => p.status !== 'Cancelado' && p.data >= inicio && p.data <= fim);
  const dias = Object.values(rows.reduce((a,p) => { const d = a[p.data] || (a[p.data] = { data:p.data, pedidos:0, total:0 }); d.pedidos++; d.total += p.total; return a; }, {})).sort((a,b) => a.data.localeCompare(b.data));
  const itens = Object.values(rows.flatMap((p) => p.itens).reduce((a,i) => { const r = a[i.nome] || (a[i.nome] = { nome:i.nome, quantidade:0, total:0 }); r.quantidade += i.quantidade; r.total += i.subtotal; return a; }, {})).sort((a,b) => b.quantidade - a.quantidade);
  return { dias, itens, total: rows.reduce((n,p) => n+p.total,0), quantidade:rows.length };
}
// Planilha XML compatível com Excel para a demonstração; a API futura poderá gerar XLSX.
export function baixarPlanilha(dias, itens) {
  const row = (cells) => '<Row>' + cells.map((c) => '<Cell><Data ss:Type="String">' + escapeXml(c) + '</Data></Cell>').join('') + '</Row>';
  const xml = '<?xml version="1.0" encoding="UTF-8"?><?mso-application progid="Excel.Sheet"?><Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"><Worksheet ss:Name="Vendas"><Table>' + row(['Data','Pedidos','Total']) + dias.map((d) => row([d.data,d.pedidos,money(d.total)])).join('') + row(['','', '']) + row(['Item','Quantidade','Total']) + itens.map((i) => row([i.nome,i.quantidade,money(i.total)])).join('') + '</Table></Worksheet></Workbook>';
  const url = URL.createObjectURL(new Blob([xml], { type: 'application/vnd.ms-excel;charset=utf-8' }));
  const link = document.createElement('a'); link.href = url; link.download = 'relatorio-cantina.xls'; link.click(); setTimeout(() => URL.revokeObjectURL(url), 1000);
}
