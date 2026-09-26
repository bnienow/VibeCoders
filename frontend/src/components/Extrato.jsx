import { dataBR, dinheiro } from '../services/formatos';

// Resumo e movimentos de um extrato da API (usado pelo aluno e pelo responsável)
export default function Extrato({ extrato }) {
  if (!extrato) return null;

  return <>
    <div className="grid-3" style={{ marginBottom: 20 }}>
      <div className="surface kpi"><span>Saldo atual</span><strong>{dinheiro(extrato.saldoAtual)}</strong></div>
      <div className="surface kpi"><span>Gasto no mês</span><strong>{dinheiro(extrato.totalGasto)}</strong></div>
      <div className="surface kpi"><span>Créditos no mês</span><strong>{dinheiro(extrato.totalCreditos)}</strong></div>
    </div>

    <div className="surface pad">
      {[...extrato.movimentos].reverse().map((m, indice) => <div className="line" key={indice}>
        <div className="stack" style={{ gap: 4 }}>
          <span className="small muted">{dataBR(m.data)} · {m.tipo}</span>
          <strong>{m.descricao}</strong>
          {m.itens.map(i => <span className="small muted" key={i.itemId}>{i.quantidade} × {i.nome} · {dinheiro(i.precoUnitario)} = {dinheiro(i.subtotal)}</span>)}
        </div>
        <div className="right"><strong>{dinheiro(m.valor)}</strong><div className="small muted">saldo {dinheiro(m.saldoApos)}</div></div>
      </div>)}
      {!extrato.movimentos.length && <div className="empty">Nenhum movimento neste mês.</div>}
    </div>
  </>;
}
