import { useState } from 'react';
import Feedback from '../components/Feedback';
import { useCantina } from '../hooks/useCantina';
import { useRelogio } from '../hooks/useRelogio';
import { dataLocal, intervalos } from '../services/catalogo';
import { statusPedido } from '../services/cantinaService';
export default function PainelIntervalo() {
  const { estado, service } = useCantina(), agora = useRelogio();
  const [busca, setBusca] = useState(''), [filtro, setFiltro] = useState('Pendentes'), [feedback, setFeedback] = useState({});
  const horario = agora.getHours() * 60 + agora.getMinutes();
  const key = horario <= 9 * 60 + 20 ? 'manha' : horario <= 15 * 60 + 50 ? 'tarde' : null;
  const intervalo = key ? intervalos[key] : null;
  const pedidos = estado.pedidos.filter((p) => p.tipoVenda === 'Antecipado' && p.data === dataLocal() && p.intervalo === key && p.status !== 'Cancelado');
  const pendentes = pedidos.filter((p) => p.status !== 'Entregue'), entregues = pedidos.filter((p) => p.status === 'Entregue');
  const visiveis = (filtro === 'Pendentes' ? pendentes : entregues).filter((p) => (p.codigoRetirada + ' ' + p.usuarioNome + ' ' + (estado.usuarios.find((u) => u.id === p.usuarioId)?.email || '')).toLowerCase().includes(busca.toLowerCase()));
  const preparo = Object.values(pendentes.flatMap((p) => p.itens).reduce((a,i) => { const item = a[i.nome] || (a[i.nome] = { nome:i.nome, quantidade:0 }); item.quantidade += i.quantidade; return a; }, {})).sort((a,b) => b.quantidade-a.quantidade);
  const [h,m] = (intervalo?.inicio || '00:00').split(':').map(Number), countdown = Math.max(0, h*60+m-horario), clock = String(Math.floor(countdown/60)).padStart(2,'0') + ':' + String(countdown%60).padStart(2,'0');
  function entregar(id) { try { service.entregarPedido(id); setFeedback({ sucesso: 'Pedido entregue.' }); } catch(e) { setFeedback({ erro:e.message }); } }
  return <><div className="section-head"><span className="eyebrow">Operação · intervalo</span><h1>Painel de preparo e retirada</h1><p className="lead">Busque pelo código, nome ou e-mail e registre a entrega com um toque.</p></div><Feedback {...feedback}/>
    <div className="grid-3" style={{margin:'20px 0'}}><div className="surface kpi"><span>Próximo intervalo</span><strong>{intervalo?.nome || 'Encerrado'}</strong></div><div className="surface kpi"><span>Começa em</span><strong className="clock">{intervalo ? clock : '—'}</strong></div><div className="surface kpi"><span>Pedidos · pendentes</span><strong>{pedidos.length} · {pendentes.length}</strong></div></div>
    {!intervalo ? <div className="empty">Não há outro intervalo hoje. O PDV de balcão permanece disponível.</div> : <div className="admin-grid"><section className="surface pad"><h2>Preparo da cozinha</h2><p className="muted small">Quantidades dos pedidos ainda não entregues.</p>{preparo.map((i) => <div className="line" key={i.nome}><strong>{i.nome}</strong><span className="badge">{i.quantidade} ×</span></div>)}{!preparo.length && <div className="empty">Nenhum item pendente.</div>}</section>
      <section className="surface pad"><div className="between wrap"><h2>Pedidos do intervalo</h2><div className="pill-tabs">{['Pendentes','Entregues'].map((v) => <button key={v} aria-pressed={filtro===v} onClick={() => setFiltro(v)}>{v}</button>)}</div></div><label className="field">Buscar código, nome ou e-mail<input className="input" type="search" value={busca} onChange={(e) => setBusca(e.target.value)} placeholder="Ex.: A7K2"/></label>
        {visiveis.map((p) => <article className="surface order-card" key={p.id}><div className="between wrap"><div><span className="eyebrow">Código {p.codigoRetirada}</span><h3 style={{margin:'7px 0'}}>{p.usuarioNome}</h3><span className="muted small">{statusPedido(p)} · {p.itens.map((i) => i.quantidade + '× ' + i.nome).join(', ')}</span></div><div className="stack">{p.temAlertaAlergia && <span className="badge red">Atenção: alérgenos</span>}{p.status !== 'Entregue' && <button className="btn" onClick={() => entregar(p.id)}>Entregar</button>}</div></div></article>)}{!visiveis.length && <div className="empty" style={{marginTop:18}}>Nenhum pedido encontrado.</div>}</section></div>}</>;
}