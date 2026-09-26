import { useState } from 'react';
import Feedback from '../components/Feedback';
import { useCantina } from '../hooks/useCantina';
import { money } from '../services/cantinaService';
export default function GestaoFilhos({ adultoId = 'adulto-1' }) {
  const { estado, service } = useCantina();
  const filhos = estado.usuarios.filter((u) => u.adultoId === adultoId);
  const [selected, setSelected] = useState('aluno-1'), [value, setValue] = useState(''), [limit, setLimit] = useState(''), [restriction, setRestriction] = useState(''), [method, setMethod] = useState(''), [feedback, setFeedback] = useState({});
  const filho = filhos.find((u) => u.id === selected) || filhos[0];
  function run(action, message) { try { action(); setFeedback({ sucesso: message }); } catch (e) { setFeedback({ erro: e.message }); } }
  return <><div className="section-head"><span className="eyebrow">Gestão da família</span><h1>Cuidados de cada filho</h1><p className="lead">Crédito, limite diário e restrições alimentares.</p></div>
    <Feedback {...feedback}/><div className="pill-tabs" style={{margin:'22px 0'}}>{filhos.map((u) => <button key={u.id} aria-pressed={filho?.id === u.id} onClick={() => { setSelected(u.id); setFeedback({}); }}>{u.nome}</button>)}</div>
    {filho ? <><div className="surface pad between wrap" style={{marginBottom:20}}><div><span className="eyebrow">{filho.turma}</span><h2>{filho.nome}</h2><p className="muted">{filho.email}</p></div><div><span className="muted small">Saldo atual</span><div className="metric">{money(filho.saldo)}</div></div></div>
      <div className="grid-3">
        <section className="surface pad stack"><h2>Adicionar crédito</h2><label className="field">Valor (R$)<input className="input" type="number" min="1" max="1000" step=".01" value={value} onChange={(e) => setValue(e.target.value)}/></label><label className="field">Método demonstrativo<select className="input" value={method} onChange={(e) => setMethod(e.target.value)}><option value="">Selecione um método</option>{estado.metodos.map((m) => <option value={m.id} key={m.id}>{m.apelido}</option>)}</select></label><button className="btn" onClick={() => { if (!method) return setFeedback({ erro: 'Selecione um método de pagamento.' }); run(() => service.adicionarCredito(filho.id, value, method), 'Crédito demonstrativo registrado.'); setValue(''); }}>Adicionar crédito</button></section>
        <section className="surface pad stack"><h2>Limite diário</h2><p className="muted small">Atual: {filho.limiteDiario == null ? 'sem limite' : money(filho.limiteDiario)}</p><label className="field">Novo limite (R$)<input className="input" type="number" min=".01" step=".01" value={limit} onChange={(e) => setLimit(e.target.value)} placeholder="Ex.: 30,00"/></label><button className="btn" onClick={() => run(() => service.definirLimite(filho.id, Number(limit)), 'Limite diário atualizado.')}>Salvar limite</button><button className="btn secondary" onClick={() => run(() => service.definirLimite(filho.id, null), 'Limite removido.')}>Deixar sem limite</button></section>
        <section className="surface pad stack"><h2>Restrições</h2><p className="muted small">Atuais: {filho.restricoes.length ? filho.restricoes.join(', ') : 'nenhuma'}</p><label className="field">Alérgenos separados por vírgula<textarea className="input" rows="3" value={restriction} onChange={(e) => setRestriction(e.target.value)} placeholder="Ex.: Lactose, Amendoim"/></label><button className="btn" onClick={() => run(() => service.definirRestricoes(filho.id, restriction), 'Restrições atualizadas.')}>Salvar restrições</button></section>
      </div></> : <div className="empty">Nenhum filho cadastrado.</div>}</>;
}
