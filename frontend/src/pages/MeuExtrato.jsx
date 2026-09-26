import { useState } from 'react';
import { useCantina } from '../hooks/useCantina';
import { displayDate, money } from '../services/cantinaService';
import { dataLocal } from '../services/catalogo';
export default function MeuExtrato() {
  const { estado, usuario } = useCantina(), aluno = usuario?.papel === 'Aluno' ? usuario : estado.usuarios.find((u) => u.id === 'aluno-1');
  const [mes,setMes] = useState(dataLocal().slice(0,7));
  const movimentos = estado.movimentos.filter((m) => m.usuarioId===aluno?.id && m.data.slice(0,7)===mes);
  return <><div className="section-head"><span className="eyebrow">Sua conta</span><h1>Meu extrato</h1><p className="lead">Compras, créditos e estornos, com os valores registrados na data.</p></div><div className="surface pad between wrap no-print" style={{marginBottom:20}}><label className="field">Mês<input className="input" type="month" value={mes} onChange={(e) => setMes(e.target.value)}/></label><button className="btn secondary" onClick={() => window.print()}>Exportar PDF (impressão)</button></div>
    <div className="surface pad"><h2>Saldo atual: {money(aluno?.saldo)}</h2>{movimentos.map((m) => <div className="line" key={m.id}><div><span className="eyebrow">{displayDate(m.data)} · {m.tipo}</span><strong style={{display:'block',marginTop:7}}>{m.descricao}</strong>{m.itens?.map((i,index) => <div className="small muted" key={index}>{i.quantidade} × {i.nome} · {money(i.precoUnitario)} = {money(i.subtotal)}</div>)}</div><div className="right"><strong>{money(m.valor)}</strong><div className="small muted">Após: {money(m.saldoApos)}</div></div></div>)}{!movimentos.length && <div className="empty">Nenhum movimento neste mês.</div>}</div><p className="hint no-print">A exportação usa a impressão do navegador; o PDF gerado pela API será integrado posteriormente.</p></>;
}
