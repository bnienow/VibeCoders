import { useState } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

const inicial = { nome:'', cpf:'', telefone:'', email:'', senha:'', confirmar:'' };
export default function CadastroAdulto() {
  const navigate = useNavigate();
  const { cadastrar } = useAuth();
  const [campos, setCampos] = useState(inicial), [erro, setErro] = useState('');
  const [ocupado, setOcupado] = useState(false), [mostrar, setMostrar] = useState(false);
  const alterar = (campo, valor) => setCampos(anterior => ({...anterior,[campo]:valor}));
  async function enviar(evento) {
    evento.preventDefault(); setErro('');
    if (campos.senha !== campos.confirmar) { setErro('As senhas não coincidem.'); return; }
    setOcupado(true);
    try { await cadastrar({ nome: campos.nome, email: campos.email, senha: campos.senha, cpf: campos.cpf, telefone: campos.telefone }); navigate('/cadastro-aluno'); }
    catch (falha) { setErro(falha.message); } finally { setOcupado(false); }
  }
  return <main className="auth-page"><section className="surface auth-card">
    <h1>Crie sua conta de adulto</h1><p className="muted">Cadastre-se para acompanhar a alimentação de um estudante.</p>
    <div className="tabs"><NavLink to="/cadastro-aluno">♧ Aluno</NavLink><NavLink to="/cadastro-adulto">♙ Adulto</NavLink></div>
    <form onSubmit={enviar}>
      <label className="field">Nome completo<input className="input" value={campos.nome} onChange={e=>alterar('nome',e.target.value)} placeholder="Ex.: Roberto Silva" required /></label>
      <label className="field">CPF<input className="input" inputMode="numeric" value={campos.cpf} onChange={e=>alterar('cpf',e.target.value)} placeholder="000.000.000-00" required /></label>
      <label className="field">Telefone<input className="input" type="tel" value={campos.telefone} onChange={e=>alterar('telefone',e.target.value)} placeholder="(00) 00000-0000" required /></label>
      <label className="field">E-mail<input className="input" type="email" value={campos.email} onChange={e=>alterar('email',e.target.value)} placeholder="voce@exemplo.com" required /></label>
      <label className="field">Senha<input className="input" type={mostrar?'text':'password'} value={campos.senha} onChange={e=>alterar('senha',e.target.value)} minLength={6} required /></label>
      <label className="field">Confirmar senha<input className="input" type={mostrar?'text':'password'} value={campos.confirmar} onChange={e=>alterar('confirmar',e.target.value)} minLength={6} required /></label>
      <label className="row small"><input type="checkbox" checked={mostrar} onChange={e=>setMostrar(e.target.checked)} /> Mostrar senhas</label>
      <p className="muted small">Ao continuar, você concorda com os termos de uso e a política de privacidade.</p>
      {erro && <div className="alert" role="alert">{erro}</div>}
      <button className="btn full" type="submit" disabled={ocupado}>{ocupado?'Criando...':'➜ Criar minha conta'}</button>
    </form><p className="auth-bottom">Já tem uma conta? <Link className="text-button" to="/login">Entrar</Link></p>
  </section></main>;
}
