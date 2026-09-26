import { useState } from 'react';
import Feedback from '../components/Feedback';
import { useApi } from '../hooks/useApi';
import { fechamentoService } from '../services/fechamentoService';
import { dataBR, dataISO, dinheiro } from '../services/formatos';
import { relatorioService } from '../services/relatorioService';

export default function Relatorios() {
  const hoje = dataISO();
  const [inicio, setInicio] = useState(hoje.slice(0, 8) + '01'), [fim, setFim] = useState(hoje), [feedback, setFeedback] = useState({});
  const { dados } = useApi(() => Promise.all([relatorioService.vendas(inicio, fim), relatorioService.itensMaisVendidos(inicio, fim)]), [inicio, fim]);
  const [dias, itens] = dados ?? [[], []];

  const total = dias.reduce((soma, d) => soma + d.total, 0);
  const quantidade = dias.reduce((soma, d) => soma + d.pedidos, 0);
  const maior = Math.max(1, ...dias.map(d => d.total));

  // Fechamento mensal (R7): gera o do mês anterior para todas as famílias que ainda não têm
  async function gerarFechamentos() {
    try {
      const { gerados } = await fechamentoService.gerar();
      setFeedback({ sucesso: `${gerados} fechamento(s) do mês anterior gerado(s).` });
    } catch (e) {
      setFeedback({ erro: e.message });
    }
  }

  return <>
    <div className="section-head"><h1>Relatórios</h1></div>
    <Feedback {...feedback}/>

    <div className="surface pad toolbar no-print" style={{ margin: '16px 0 20px' }}>
      <label className="field">De<input className="input" type="date" value={inicio} max={fim} onChange={e => setInicio(e.target.value)}/></label>
      <label className="field">Até<input className="input" type="date" value={fim} min={inicio} max={hoje} onChange={e => setFim(e.target.value)}/></label>
      <a className="btn secondary" href={relatorioService.urlExcel(inicio, fim)}>Exportar Excel</a>
      <button className="btn soft" onClick={gerarFechamentos}>Gerar fechamento do mês anterior</button>
    </div>

    <div className="grid-2" style={{ marginBottom: 20 }}>
      <div className="surface kpi"><span>Faturamento</span><strong>{dinheiro(total)}</strong></div>
      <div className="surface kpi"><span>Vendas</span><strong>{quantidade}</strong></div>
    </div>

    <div className="grid-2 align-start">
      <section className="surface pad">
        <h2>Por dia</h2>
        {dias.map(d => <div key={d.data} style={{ margin: '14px 0' }}>
          <div className="between small"><span>{dataBR(d.data)} · {d.pedidos} pedido(s)</span><strong>{dinheiro(d.total)}</strong></div>
          <div className="bar" style={{ marginTop: 6 }}><i style={{ width: `${d.total / maior * 100}%` }}/></div>
        </div>)}
        {dados && !dias.length && <div className="empty">Sem vendas no período.</div>}
      </section>
      <section className="surface pad">
        <h2>Mais vendidos</h2>
        {itens.map((i, indice) => <div className="line" key={i.nome}>
          <span><strong>{indice + 1}. {i.nome}</strong><span className="small muted"> · {i.quantidade} un.</span></span>
          <strong>{dinheiro(i.total)}</strong>
        </div>)}
        {dados && !itens.length && <div className="empty">Sem itens no período.</div>}
      </section>
    </div>
  </>;
}
