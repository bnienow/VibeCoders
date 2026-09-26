import { useState } from 'react';
import { useCantina } from '../hooks/useCantina';

export default function Login({ navigate }) {
  const { service } = useCantina();
  const [email, setEmail] = useState(''), [senha, setSenha] = useState('');
  const [mostrar, setMostrar] = useState(false), [erro, setErro] = useState(''), [ocupado, setOcupado] = useState(false);
  async function enviar(evento) {
    evento.preventDefault(); setErro(''); setOcupado(true);
    try { const usuario = await service.entrar(email, senha); navigate(usuario.papel === 'Aluno' ? '/home' : '/cadastro-aluno'); }
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
    <p className="auth-bottom">Ainda não tem conta? <button className="text-button" onClick={() => navigate('/cadastro-adulto')}>Cadastre-se</button></p>
    <p className="auth-bottom small">Demonstração: marina@aluno.cantina.test ou carla@email.test • senha123</p>
  </section></main>;
}
