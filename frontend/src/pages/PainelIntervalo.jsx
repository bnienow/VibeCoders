import { useState } from 'react';
import Feedback from '../components/Feedback';
import { useApi } from '../hooks/useApi';
import { useRelogio } from '../hooks/useRelogio';
import { hora } from '../services/formatos';
import { painelService } from '../services/painelService';
import { pedidoService } from '../services/pedidoService';

export default function PainelIntervalo() {
  const agora = useRelogio(); // muda a cada 15 s: recarrega painel e preparo sozinho
  const { dados: painel, recarregar } = useApi(() => painelService.intervaloAtual(), [agora]);
  const { dados: preparo, recarregar: recarregarPreparo } = useApi(() => painelService.preparo(), [agora]);
  const [busca, setBusca] = useState(''), [filtro, setFiltro] = useState('Pendentes'), [feedback, setFeedback] = useState({});

  // Com texto na busca, procura na API por código, nome ou e-mail entre os pedidos de hoje
  const { dados: encontrados } = useApi(() => busca.trim() ? painelService.buscar(busca.trim()) : Promise.resolve(null), [busca]);

  const pedidos = painel?.pedidos ?? [];
  const visiveis = encontrados ?? pedidos.filter(p => (filtro === 'Entregues') === (p.status === 'Entregue'));

  // Contagem regressiva até o início do intervalo
  const [h, m] = hora(painel?.horaInicio || '00:00').split(':').map(Number);
  const minutos = Math.max(0, h * 60 + m - (agora.getHours() * 60 + agora.getMinutes()));
  const relogio = `${String(Math.floor(minutos / 60)).padStart(2, '0')}:${String(minutos % 60).padStart(2, '0')}`;

  async function entregar(id) {
    try {
      await pedidoService.entregar(id);
      setFeedback({ sucesso: 'Pedido entregue.' });
      recarregar();
      recarregarPreparo();
    } catch (e) {
      setFeedback({ erro: e.message });
    }
  }

  if (painel && !painel.intervalo) return <><div className="section-head"><h1>Painel do intervalo</h1></div><div className="empty">Não há mais intervalo hoje.</div></>;

  return <>
    <div className="section-head"><h1>Painel do intervalo</h1></div>
    <Feedback {...feedback}/>

    <div className="grid-3" style={{ margin: '16px 0 20px' }}>
      <div className="surface kpi"><span>Intervalo</span><strong>{painel?.intervalo} · {hora(painel?.horaInicio)}</strong></div>
      <div className="surface kpi"><span>Começa em</span><strong className="clock">{relogio}</strong></div>
      <div className="surface kpi"><span>Pendentes · entregues</span><strong>{painel?.pendentes ?? 0} · {painel?.entregues ?? 0}</strong></div>
    </div>

    <div className="admin-grid">
      <section className="surface pad">
        <h2>Preparo</h2>
        {preparo?.map(i => <div className="line" key={i.nome}><span>{i.nome}</span><span className="badge">{i.quantidade}×</span></div>)}
        {preparo && !preparo.length && <div className="empty">Nada pendente.</div>}
      </section>

      <section className="surface pad">
        <div className="between wrap" style={{ marginBottom: 14 }}>
          <h2 style={{ margin: 0 }}>Pedidos</h2>
          <div className="pill-tabs">{['Pendentes', 'Entregues'].map(v => <button key={v} aria-pressed={filtro === v} onClick={() => setFiltro(v)}>{v}</button>)}</div>
        </div>
        <label className="field">Buscar por código, nome ou e-mail<input className="input" type="search" value={busca} onChange={e => setBusca(e.target.value)}/></label>

        {visiveis.map(p => <article className="surface order-card" key={p.id}>
          <div className="between wrap">
            <div className="stack" style={{ gap: 4 }}>
              <div className="row"><strong>{p.codigoRetirada}</strong><span>{p.usuarioNome}</span>{p.temAlertaAlergia && <span className="badge red">Alérgenos</span>}</div>
              <span className="small muted">{p.itens.map(i => `${i.quantidade}× ${i.nome}`).join(', ')}</span>
            </div>
            {p.status !== 'Entregue' ? <button className="btn small" onClick={() => entregar(p.id)}>Entregar</button> : <span className="badge">Entregue</span>}
          </div>
        </article>)}
        {!visiveis.length && <div className="empty" style={{ marginTop: 14 }}>Nenhum pedido encontrado.</div>}
      </section>
    </div>
  </>;
}
