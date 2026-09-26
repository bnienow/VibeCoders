import { useState } from 'react';
import Feedback from '../components/Feedback';
import { useCantina } from '../hooks/useCantina';
import { money } from '../services/cantinaService';
export default function FechamentoMensal({ adultoId = 'adulto-1' }) {
  const { estado, service } = useCantina();
  const [metodo, setMetodo] = useState(''), [feedback, setFeedback] = useState({});
  const fechamentos = estado.fechamentos.filter((f) => f.adultoId === adultoId);
  function pagar(id) { try { service.pagarFechamento(id, metodo); setFeedback({ sucesso: 'Pagamento demonstrativo registrado.' }); } catch (e) { setFeedback({ erro: e.message }); } }
  return <><div className="section-head"><span className="eyebrow">Consolidado mensal</span><h1>Fechamentos da família</h1><p className="lead">O consumo do mês é informativo. O valor a pagar corresponde ao fiado apurado no fechamento.</p></div><Feedback {...feedback}/>
    <div className="stack" style={{marginTop:20}}>{fechamentos.map((f) => <article className="surface pad" key={f.id}><div className="between wrap"><div><span className="eyebrow">Referência {f.mesReferencia.slice(5)}/{f.mesReferencia.slice(0,4)}</span><h2 style={{margin:'8px 0'}}>Fechamento mensal</h2><span className={'badge ' + (f.status === 'Aberto' ? 'amber' : '')}>{f.status}</span></div><div className="right"><span className="muted small">Fiado a pagar</span><div className="metric">{money(f.valorTotal)}</div></div></div><div className="divider"/>
      {f.consumo.map((c,index) => <div key={index}><strong>{c.nome} · consumo informativo {money(c.total)}</strong>{c.itens.map((i,j) => <div className="line small" key={j}><span>{i.quantidade} × {i.nome}</span><span>{money(i.total)}</span></div>)}</div>)}
      {f.status === 'Aberto' && <div className="row wrap no-print" style={{marginTop:22}}><label className="field">Método fictício<select className="input" value={metodo} onChange={(e) => setMetodo(e.target.value)}><option value="">Selecione</option>{estado.metodos.map((m) => <option key={m.id} value={m.id}>{m.apelido}</option>)}</select></label><button className="btn" onClick={() => pagar(f.id)}>Pagar (simulação)</button></div>}
    </article>)}{!fechamentos.length && <div className="empty">Ainda não há fechamento gerado para este responsável.</div>}</div></>;
}
