import { useState } from 'react';
import Layout from '../components/Layout';
import { useCantina } from '../hooks/useCantina';
import { displayDate, money } from '../services/cantinaService';
import { dataLocal } from '../services/catalogo';
export default function ExtratoFilho({ adultoId = 'adulto-1' }) {
  const { estado } = useCantina(), adulto = estado.usuarios.find((u) => u.id === adultoId);
  const filhos = estado.usuarios.filter((u) => u.adultoId === adultoId);
  const [selected, setSelected] = useState('aluno-1'), [mes, setMes] = useState(dataLocal().slice(0,7));
  const filho = filhos.find((u) => u.id === selected) || filhos[0];
  const movimentos = estado.movimentos.filter((m) => m.usuarioId === filho?.id && m.data.slice(0,7) === mes).sort((a,b) => b.data.localeCompare(a.data));
  const gasto = movimentos.filter((m) => m.tipo === 'Compra').reduce((n,m) => n - m.valor,0);
  return <Layout perfil="Responsável" nome={adulto?.nome}><div className="section-head"><span className="eyebrow">Transparência</span><h1>Extrato do filho</h1><p className="lead">Consulte os movimentos e os itens comprados, com o preço registrado na data.</p></div>
    <div className="surface pad between wrap no-print" style={{marginBottom:20}}><label className="field">Aluno<select className="input" value={filho?.id || ''} onChange={(e) => setSelected(e.target.value)}>{filhos.map((u) => <option value={u.id} key={u.id}>{u.nome}</option>)}</select></label><label className="field">Mês<input className="input" type="month" value={mes} onChange={(e) => setMes(e.target.value)}/></label><button className="btn secondary" onClick={() => window.print()}>Exportar PDF (impressão)</button></div>
    <div className="grid-2" style={{marginBottom:20}}><div className="surface kpi"><span>Compras no período</span><strong>{money(gasto)}</strong></div><div className="surface kpi"><span>Saldo atual</span><strong>{money(filho?.saldo)}</strong></div></div>
    <div className="stack">{movimentos.map((m) => <article className="surface pad" key={m.id}><div className="between wrap"><div><span className="eyebrow">{displayDate(m.data)} · {m.tipo}</span><h3 style={{margin:'8px 0'}}>{m.descricao}</h3></div><div className="right"><strong className="metric">{money(m.valor)}</strong><div className="small muted">Saldo após: {money(m.saldoApos)}</div></div></div>{m.itens?.length > 0 && <div className="divider"/>}{m.itens?.map((i, index) => <div className="line small" key={index}><span>{i.quantidade} × {i.nome} · {money(i.precoUnitario)} cada</span><strong>{money(i.subtotal)}</strong></div>)}</article>)}{!movimentos.length && <div className="empty">Nenhum movimento neste mês.</div>}</div>
    <p className="hint no-print" style={{marginTop:18}}>A exportação usa a impressão do navegador. A geração do PDF pela API será ligada posteriormente.</p></Layout>;
}