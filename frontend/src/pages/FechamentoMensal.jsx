import { useState } from 'react';
import Feedback from '../components/Feedback';
import { useApi } from '../hooks/useApi';
import { useAuth } from '../hooks/useAuth';
import { fechamentoService } from '../services/fechamentoService';
import { dataBR, dinheiro } from '../services/formatos';

export default function FechamentoMensal() {
  const { usuario } = useAuth();
  const { dados: fechamentos, recarregar } = useApi(() => fechamentoService.doAdulto(usuario.id), [usuario.id]);
  const [feedback, setFeedback] = useState({});

  async function pagar(id) {
    try {
      await fechamentoService.pagar(id);
      setFeedback({ sucesso: 'Pagamento registrado. O fiado da família foi quitado.' });
      recarregar();
    } catch (e) {
      setFeedback({ erro: e.message });
    }
  }

  return <>
    <div className="section-head"><h1>Fechamentos</h1></div>
    <Feedback {...feedback}/>

    <div className="stack" style={{ marginTop: 16 }}>{fechamentos?.map(f => <article className="surface pad" key={f.id}>
      <div className="between wrap">
        <div className="row"><h2 style={{ margin: 0 }}>{dataBR(f.mesReferencia).slice(3)}</h2><span className={`badge ${f.status === 'Aberto' ? 'amber' : ''}`}>{f.status}</span></div>
        <div className="right"><span className="small muted">A pagar (fiado)</span><div className="metric">{dinheiro(f.valorTotal)}</div></div>
      </div>
      <div className="divider"/>
      {f.consumo.map(c => <div key={c.nome} style={{ marginBottom: 12 }}>
        <div className="between"><strong>{c.nome}</strong><span className="small muted">consumo {dinheiro(c.total)}</span></div>
        {c.itens.map(i => <div className="line small" key={i.nome}><span>{i.quantidade} × {i.nome}</span><span>{dinheiro(i.total)}</span></div>)}
      </div>)}
      {f.status === 'Aberto' && <div className="no-print" style={{ marginTop: 12 }}><button className="btn" onClick={() => pagar(f.id)}>Pagar {dinheiro(f.valorTotal)}</button></div>}
    </article>)}
    {fechamentos && !fechamentos.length && <div className="empty">Nenhum fechamento gerado.</div>}</div>
  </>;
}
