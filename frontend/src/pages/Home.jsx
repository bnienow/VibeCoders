import { Link } from 'react-router-dom';
import { useApi } from '../hooks/useApi';
import { useAuth } from '../hooks/useAuth';
import { alunoService } from '../services/alunoService';
import { dataBR, dinheiro } from '../services/formatos';
import { pedidoService } from '../services/pedidoService';

export default function Home() {
  const { usuario } = useAuth();
  const { dados } = useApi(() => Promise.all([alunoService.detalhe(usuario.id), pedidoService.meus()]), [usuario.id]);
  const [aluno, pedidos] = dados ?? [];

  // Pedido antecipado que ainda vai ser retirado
  const pendente = pedidos?.find(p => p.tipoVenda === 'Antecipado' && ['Aberto', 'Confirmado'].includes(p.status));

  return <>
    <div className="section-head"><h1>Olá, {usuario.nome.split(' ')[0]}</h1></div>

    {pendente && <div className="note" style={{ marginBottom: 20 }}>
      Pedido para {pendente.intervalo} de {dataBR(pendente.data)} ({pendente.status}) · código de retirada <strong>{pendente.codigoRetirada}</strong>
    </div>}

    {aluno && <div className="grid-3" style={{ marginBottom: 24 }}>
      <div className="surface kpi"><span>Saldo</span><strong>{dinheiro(aluno.saldo)}</strong>{aluno.saldo < 0 && <span className="badge amber">Fiado: {dinheiro(-aluno.saldo)} de R$ 250,00</span>}</div>
      <div className="surface kpi"><span>Gasto no mês</span><strong>{dinheiro(aluno.gastoMes)}</strong></div>
      <div className="surface kpi"><span>Limite diário</span><strong>{aluno.limiteDiario == null ? 'Sem limite' : dinheiro(aluno.limiteDiario)}</strong></div>
    </div>}

    <div className="row wrap">
      <Link className="btn" to="/cardapio">Fazer pedido</Link>
      <Link className="btn secondary" to="/historico">Meus pedidos</Link>
      <Link className="btn secondary" to="/extrato">Meu extrato</Link>
    </div>
  </>;
}
