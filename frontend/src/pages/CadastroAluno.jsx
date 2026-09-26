import { useState } from 'react';
import { useCantina } from '../hooks/useCantina';

const inicial = { nome:'', email:'', turma:'', nascimento:'', restricoes:'', senha:'', confirmar:'' };
export default function CadastroAluno({ navigate }) {
  const { usuario, service } = useCantina();
  const [campos, setCampos] = useState(inicial), [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState(''), [ocupado, setOcupado] = useState(false);
  const alterar = (campo, valor) => setCampos(anterior => ({...anterior,[campo]:valor}));
  async function enviar(evento) {
    evento.preventDefault(); setErro(''); setSucesso('');
    if (campos.senha !== campos.confirmar) { setErro('As senhas não coincidem.'); return; }
    setOcupado(true);
    try { await service.cadastrarAluno(campos); setCampos(inicial); setSucesso('Aluno cadastrado e vinculado ao responsável. Ele já pode entrar com seu e-mail e senha.'); }
    catch (falha) { setErro(falha.message); } finally { setOcupado(false); }
  }
  return <main className="auth-page"><section className="surface auth-card">
    <h1>Crie sua conta de aluno</h1><p className="muted">Peça seus favoritos e aproveite o intervalo do seu jeito.</p>
    <div className="tabs"><button type="button" className="active">♧ Aluno</button><button type="button" onClick={() => navigate('/cadastro-adulto')}>♙ Adulto</button></div>
    {usuario?.papel !== 'Adulto' ? <div className="stack" style={{marginTop:20}}>
      <div className="alert">O cadastro de aluno é feito pelo responsável. Entre com a conta de adulto para vincular o estudante.</div>
      <button className="btn full" onClick={() => navigate('/login')}>Entrar como responsável</button>
      <button className="btn secondary full" onClick={() => navigate('/cadastro-adulto')}>Criar conta de adulto</button>
    </div> : <form onSubmit={enviar}>
      <p className="small muted">Responsável: <strong>{usuario.nome}</strong></p>
      <label className="field">Nome completo<input className="input" value={campos.nome} onChange={e=>alterar('nome',e.target.value)} placeholder="Ex.: Marina Silva" required /></label>
      <label className="field">E-mail institucional<input className="input" type="email" value={campos.email} onChange={e=>alterar('email',e.target.value)} placeholder="nome@aluno.cantina.test" required /></label>
      <div className="grid-2"><label className="field">Turma<input className="input" value={campos.turma} onChange={e=>alterar('turma',e.target.value)} placeholder="Ex.: 8º ano" required /></label>
      <label className="field">Data de nascimento<input className="input" type="date" value={campos.nascimento} onChange={e=>alterar('nascimento',e.target.value)} required /></label></div>
      <label className="field">Restrições alimentares<input className="input" value={campos.restricoes} onChange={e=>alterar('restricoes',e.target.value)} placeholder="Ex.: Lactose, Amendoim (opcional)" /></label>
      <label className="field">Senha<input className="input" type="password" value={campos.senha} onChange={e=>alterar('senha',e.target.value)} minLength={6} required /></label>
      <label className="field">Confirmar senha<input className="input" type="password" value={campos.confirmar} onChange={e=>alterar('confirmar',e.target.value)} minLength={6} required /></label>
      {erro && <div className="alert" role="alert">{erro}</div>}{sucesso && <div className="success" role="status">{sucesso}</div>}
      <button className="btn full" type="submit" disabled={ocupado}>{ocupado?'Cadastrando...':'➜ Criar conta do aluno'}</button>
    </form>}
    <p className="auth-bottom">Já tem uma conta? <button className="text-button" onClick={() => navigate('/login')}>Entrar</button></p>
  </section></main>;
}
