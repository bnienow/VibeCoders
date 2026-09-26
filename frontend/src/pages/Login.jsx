import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export default function Login() {
  const navigate = useNavigate();
  const { entrar } = useAuth();
  const [email, setEmail] = useState(''), [senha, setSenha] = useState('');
  const [mostrar, setMostrar] = useState(false), [erro, setErro] = useState(''), [ocupado, setOcupado] = useState(false);
  async function enviar(evento) {
    evento.preventDefault(); setErro(''); setOcupado(true);
    try { const usuario = await entrar(email, senha); navigate(usuario.permissao === 'Adulto' ? '/cadastro-aluno' : '/home'); }
    catch (falha) { setErro(falha.message); } finally { setOcupado(false); }
  }
  return <main className="auth-page"><section className="surface auth-card">
    <h1>Bem-vindo de volta</h1><p className="muted">Entre para pedir seu lanche sem filas e acompanhar seu saldo.</p>
    <form onSubmit={enviar}>
      <label className="field">E-mail<input className="input" type="email" autoComplete="email" placeholder="voce@exemplo.com" value={email} onChange={e => setEmail(e.target.value)} required /></label>
      <label className="field">Senha<span className="row"><input className="input" type={mostrar?'text':'password'} autoComplete="current-password" placeholder="Sua senha" value={senha} onChange={e => setSenha(e.target.value)} required /><button type="button" className="btn secondary" onClick={() => setMostrar(!mostrar)}>{mostrar?'Ocultar':'Ver'}</button></span></label>
      <div className="between small"><label className="row"><input type="checkbox" defaultChecked /> Lembrar de mim</label><button type="button" className="text-button" onClick={() => setErro('Para recuperar o acesso, procure a cantina ou o responsável pela conta.')}>Esqueci minha senha</button></div>
      {erro && <div className="alert" role="alert">{erro}</div>}
      <button className="btn full" type="submit" disabled={ocupado}>{ocupado?'Entrando...':'➜ Entrar'}</button>
    </form>
    <p className="auth-bottom">Ainda não tem conta? <Link className="text-button" to="/cadastro-adulto">Cadastre-se</Link></p>
    <p className="auth-bottom small">Demonstração: lucas.andrade@aluno.cantina.test ou carla.andrade@email.test • senha123</p>
  </section></main>;
}
