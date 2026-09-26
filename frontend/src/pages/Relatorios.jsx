import { useState } from 'react';
import Layout from '../components/Layout';
import { useCantina } from '../hooks/useCantina';
import { money, displayDate } from '../services/cantinaService';
import { dataLocal } from '../services/catalogo';
import { baixarPlanilha, resumirVendas } from '../services/relatorios';
export default function Relatorios() {
  const { estado } = useCantina(), today = dataLocal();
  const [inicio, setInicio] = useState(today.slice(0,8)+'01'), [fim, setFim] = useState(today);
  const { dias, itens, total, quantidade } = resumirVendas(estado.pedidos, inicio, fim), max = Math.max(1, ...dias.map((d) => d.total));
  return <Layout perfil="Admin" nome="Relatórios"><div className="section-head"><span className="eyebrow">Resultados da cantina</span><h1>Vendas por período</h1><p className="lead">Resumo demonstrativo de pedidos antecipados e vendas de balcão.</p></div>
    <div className="surface pad row wrap no-print" style={{marginBottom:20}}><label className="field">De<input className="input" type="date" value={inicio} max={fim} onChange={(e) => setInicio(e.target.value)}/></label><label className="field">Até<input className="input" type="date" value={fim} min={inicio} onChange={(e) => setFim(e.target.value)}/></label><button className="btn secondary" disabled={!dias.length} onClick={() => baixarPlanilha(dias,itens)}>Exportar Excel (.xls)</button></div>
    <div className="grid-2" style={{marginBottom:20}}><div className="surface kpi"><span>Faturamento do período</span><strong>{money(total)}</strong></div><div className="surface kpi"><span>Vendas</span><strong>{quantidade}</strong></div></div>
    <div className="grid-2"><section className="surface pad"><h2>Por dia</h2>{dias.map((d) => <div key={d.data} style={{margin:'20px 0'}}><div className="between small"><span>{displayDate(d.data)} · {d.pedidos} pedidos</span><strong>{money(d.total)}</strong></div><div className="bar" style={{marginTop:8}}><i style={{width:(d.total/max*100)+'%'}}/></div></div>)}{!dias.length && <div className="empty">Sem vendas nesse período.</div>}</section><section className="surface pad"><h2>Mais vendidos</h2>{itens.map((i,index) => <div className="line" key={i.nome}><span><strong>{index+1}. {i.nome}</strong><div className="small muted">{i.quantidade} unidade(s)</div></span><strong>{money(i.total)}</strong></div>)}{!itens.length && <div className="empty">Sem itens no período.</div>}</section></div><p className="hint" style={{marginTop:18}}>Exportação local em planilha XML compatível com Excel; o XLSX oficial será fornecido pela API posteriormente.</p></Layout>;
}
