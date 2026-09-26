import { useMemo, useState } from 'react';
import Layout from '../components/Layout';
import Feedback from '../components/Feedback';
import Quantidade from '../components/Quantidade';
import { useCantina } from '../hooks/useCantina';
import { money } from '../services/cantinaService';
import { dataLocal } from '../services/catalogo';
export default function PdvBalcao() {
  const { estado, service } = useCantina();
  const [category, setCategory] = useState('Salgados'), [quantities, setQuantities] = useState({}), [query, setQuery] = useState(''), [userId, setUserId] = useState(''), [feedback, setFeedback] = useState({});
  const items = estado.itens.filter((i) => i.ativo && i.estoque > 0 && i.categoria !== 'Combos');
  const filtered = items.filter((i) => i.categoria === category), users = estado.usuarios.filter((u) => ['Aluno','Adulto'].includes(u.papel) && (u.nome + ' ' + u.email).toLowerCase().includes(query.toLowerCase())).slice(0,8);
  const selected = estado.usuarios.find((u) => u.id === userId);
  const lines = items.filter((i) => quantities[i.id]), total = useMemo(() => lines.reduce((n,i) => n + i.preco * quantities[i.id],0), [lines, quantities]);
  const spentToday = estado.pedidos.filter((p) => p.usuarioId === selected?.id && p.data === dataLocal() && p.status !== 'Cancelado').reduce((n,p) => n + p.total,0);
  const dailyBlocked = selected?.papel === 'Aluno' && selected.limiteDiario != null && spentToday + total > selected.limiteDiario;
  const blocked = selected?.papel === 'Aluno' && selected.saldo - total < -250;
  const allergy = lines.flatMap((i) => i.alergenos.filter((a) => selected?.restricoes?.some((r) => r.toLowerCase() === a.toLowerCase())));
  function change(id, n) { setQuantities((q) => ({...q, [id]:n})); }
  function finish(forma) {
    try { const p = service.venderBalcao(userId, quantities, forma); setQuantities({}); setQuery(''); setUserId(''); setFeedback({ sucesso: 'Venda de balcão ' + p.id + ' registrada como Entregue. ' + money(p.total) }); }
    catch(e) { setFeedback({ erro:e.message }); }
  }
  return <Layout perfil="Admin" nome="Caixa · balcão"><div className="section-head"><span className="eyebrow">Atendimento rápido</span><h1>PDV de balcão</h1><p className="lead">Venda vinculada a uma pessoa e entregue no ato, independente do horário do intervalo.</p></div><Feedback {...feedback}/>
    <div className="two-column" style={{marginTop:20}}><section><div className="pill-tabs" style={{marginBottom:16}}>{['Salgados','Doces','Bebidas'].map((v) => <button key={v} aria-pressed={category===v} onClick={() => setCategory(v)}>{v}</button>)}</div><div className="grid-3">{filtered.map((i) => <button key={i.id} type="button" className="surface pad" style={{textAlign:'left',minHeight:106}} onClick={() => change(i.id, Math.min(i.estoque, (quantities[i.id] || 0)+1))}><strong>{i.nome}</strong><div className="price" style={{marginTop:9}}>{money(i.preco)}</div><span className="small muted">Toque para adicionar · estoque {i.estoque}</span></button>)}</div></section>
      <aside className="surface pad sticky-panel"><h2>Atendimento atual</h2><label className="field">Buscar usuário por nome ou e-mail<input className="input" type="search" value={query} onChange={(e) => { setQuery(e.target.value); setUserId(''); }} placeholder="Quem está comprando?"/></label>{query && <div className="stack" style={{gap:5,marginTop:10}}>{users.map((u) => <button key={u.id} className="btn secondary full" onClick={() => { setUserId(u.id); setQuery(u.nome); }}>{u.nome} · {u.email}</button>)}{!users.length && <span className="muted small">Nenhum usuário encontrado.</span>}</div>}
        {selected && <div className="note" style={{marginTop:15}}><strong>{selected.nome}</strong><div className="small">Saldo {money(selected.saldo)} · limite diário {selected.limiteDiario == null ? '—' : money(selected.limiteDiario)}</div><div className="small">Restrições: {selected.restricoes?.join(', ') || 'nenhuma'}</div></div>}
        <div className="divider"/>{lines.map((i) => <div className="line" key={i.id}><div><strong>{i.nome}</strong><div className="small muted">{money(i.preco)} por unidade</div></div><Quantidade nome={i.nome} valor={quantities[i.id]} maximo={i.estoque} aoMudar={(n) => change(i.id,n)}/></div>)}{!lines.length && <p className="muted">Toque nos itens para montar a venda.</p>}
        <div className="between" style={{margin:'20px 0'}}><strong>Total</strong><strong className="metric">{money(total)}</strong></div>{allergy.length > 0 && <div className="alert">Alergia: contém {Array.from(new Set(allergy)).join(', ')}.</div>}{dailyBlocked && <div className="alert" style={{marginTop:10}}>Limite diário de {money(selected.limiteDiario)} atingido</div>}{blocked && <div className="alert" style={{marginTop:10}}>Limite de R$ 250,00 atingido — somente à vista</div>}
        <div className="grid-2" style={{marginTop:15,gap:8}}><button className="btn full" disabled={!selected || !lines.length || blocked || dailyBlocked} onClick={() => finish('Conta')}>Lançar na conta</button><button className="btn secondary full" disabled={!selected || !lines.length} onClick={() => finish('AVista')}>Pagou à vista</button></div></aside></div></Layout>;
}
