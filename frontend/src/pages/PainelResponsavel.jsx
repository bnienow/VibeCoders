import { useCantina } from '../hooks/useCantina';
import { money } from '../services/cantinaService';
import { dataLocal } from '../services/catalogo';
export default function PainelResponsavel({ adultoId = 'adulto-1' }) {
  const { estado } = useCantina();
  const filhos = estado.usuarios.filter((u) => u.adultoId === adultoId);
  const fechamento = estado.fechamentos.find((f) => f.adultoId === adultoId && f.status === 'Aberto');
  return <><div className="section-head"><span className="eyebrow">Visão da família</span><h1>Acompanhe cada filho</h1><p className="lead">Saldo, consumo e limites reunidos em um lugar.</p></div>
    {fechamento && <div className="note" style={{marginBottom:20}}>Fechamento de {fechamento.mesReferencia.slice(5)}/{fechamento.mesReferencia.slice(0,4)} em aberto: <strong>{money(fechamento.valorTotal)}</strong>. O valor representa o fiado da família; o consumo é apenas informativo.</div>}
    <div className="grid-2">{filhos.map((f) => {
      const gastoMes = estado.movimentos.filter((m) => m.usuarioId === f.id && m.data.slice(0,7) === dataLocal().slice(0,7) && ['Compra','Estorno'].includes(m.tipo)).reduce((n,m) => n - m.valor,0);
      return <article className="surface pad" key={f.id}><span className="eyebrow">{f.turma}</span><h2 style={{margin:'10px 0'}}>{f.nome}</h2><div className="grid-2"><div><span className="muted small">Saldo atual</span><div className="metric">{money(f.saldo)}</div>{f.saldo < 0 && <span className="badge amber">Fiado: {money(-f.saldo)} de R$ 250,00</span>}</div><div><span className="muted small">Gasto no mês</span><div className="metric">{money(gastoMes)}</div><span className="small muted">Limite diário: {f.limiteDiario == null ? 'não definido' : money(f.limiteDiario)}</span></div></div>{f.saldo < 10 && <div className="alert" style={{marginTop:20}}>Saldo baixo: acompanhe as compras deste aluno.</div>}</article>;
    })}</div>{!filhos.length && <div className="empty">Nenhum aluno vinculado a este responsável.</div>}</>;
}
