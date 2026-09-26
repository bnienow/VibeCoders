import { useApi } from '../hooks/useApi';
import { useAuth } from '../hooks/useAuth';
import { alunoService } from '../services/alunoService';
import { dataBR, dinheiro } from '../services/formatos';

export default function Perfil() {
  const { usuario } = useAuth();
  const { dados: aluno } = useApi(() => alunoService.detalhe(usuario.id), [usuario.id]);

  return <>
    <div className="section-head"><h1>Meu perfil</h1></div>
    {aluno && <div className="grid-2 align-start">
      <section className="surface pad">
        <h2>Dados</h2>
        <div className="line"><span className="muted">Nome</span><strong>{aluno.nome}</strong></div>
        <div className="line"><span className="muted">E-mail</span><strong>{aluno.email}</strong></div>
        <div className="line"><span className="muted">Turma</span><strong>{aluno.turma}</strong></div>
        <div className="line"><span className="muted">Nascimento</span><strong>{dataBR(aluno.dataNascimento)}</strong></div>
      </section>
      <section className="surface pad">
        <h2>Conta</h2>
        <div className="line"><span className="muted">Saldo</span><strong>{dinheiro(aluno.saldo)}</strong></div>
        {aluno.saldo < 0 && <div className="line"><span className="muted">Fiado</span><span className="badge amber">{dinheiro(-aluno.saldo)} de R$ 250,00</span></div>}
        <div className="line"><span className="muted">Gasto no mês</span><strong>{dinheiro(aluno.gastoMes)}</strong></div>
        <div className="line"><span className="muted">Limite diário</span><strong>{aluno.limiteDiario == null ? 'Sem limite' : dinheiro(aluno.limiteDiario)}</strong></div>
        <div className="line"><span className="muted">Restrições</span><strong>{aluno.restricoes.join(', ') || 'Nenhuma'}</strong></div>
      </section>
    </div>}
  </>;
}
