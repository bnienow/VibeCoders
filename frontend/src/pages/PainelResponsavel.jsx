import { useApi } from '../hooks/useApi';
import { useAuth } from '../hooks/useAuth';
import { alunoService } from '../services/alunoService';
import { fechamentoService } from '../services/fechamentoService';
import { dataBR, dinheiro } from '../services/formatos';

const SALDO_BAIXO = 10;

export default function PainelResponsavel() {
  const { usuario } = useAuth();
  const { dados } = useApi(() => Promise.all([alunoService.filhos(usuario.id), fechamentoService.doAdulto(usuario.id)]), [usuario.id]);
  const [filhos, fechamentos] = dados ?? [];
  const fechamento = fechamentos?.find(f => f.status === 'Aberto');

  return <>
    <div className="section-head"><h1>Família</h1></div>
    {fechamento && <div className="note" style={{ marginBottom: 20 }}>Fechamento de {dataBR(fechamento.mesReferencia).slice(3)} em aberto: <strong>{dinheiro(fechamento.valorTotal)}</strong></div>}

    <div className="grid-2">{filhos?.map(f => <article className="surface pad" key={f.id}>
      <div className="between"><h2 style={{ margin: 0 }}>{f.nome}</h2><span className="small muted">{f.turma}</span></div>
      <div className="divider"/>
      <div className="line"><span className="muted">Saldo</span><strong>{dinheiro(f.saldo)}</strong></div>
      <div className="line"><span className="muted">Gasto no mês</span><strong>{dinheiro(f.gastoMes)}</strong></div>
      <div className="line"><span className="muted">Limite diário</span><strong>{f.limiteDiario == null ? 'Sem limite' : dinheiro(f.limiteDiario)}</strong></div>
      <div className="row wrap" style={{ marginTop: 12 }}>
        {f.saldo < 0 && <span className="badge amber">Fiado: {dinheiro(-f.saldo)} de R$ 250,00</span>}
        {f.saldo < SALDO_BAIXO && <span className="badge red">Saldo baixo</span>}
      </div>
    </article>)}</div>
    {filhos && !filhos.length && <div className="empty">Nenhum filho cadastrado.</div>}
  </>;
}
