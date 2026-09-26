import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { limparEmail } from '../services/formatos';
import { PAGINA_INICIAL } from '../rotas';

export default function Login() {
  const navigate = useNavigate();
  const { entrar } = useAuth();
  const [email, setEmail] = useState(''), [senha, setSenha] = useState('');
  const [mostrar, setMostrar] = useState(false), [erro, setErro] = useState(''), [ocupado, setOcupado] = useState(false);

  async function enviar(evento) {
    evento.preventDefault();
    setErro('');
    setOcupado(true);
    try {
      const usuario = await entrar(email, senha);
      navigate(PAGINA_INICIAL[usuario.permissao]);
    } catch (falha) {
      setErro(falha.message);
    } finally {
      setOcupado(false);
    }
  }

  return <main className="auth"><section className="surface auth-card">
    <h1>Entrar</h1>
    <form onSubmit={enviar}>
      <label className="field">E-mail
        <input className="input" type="email" autoComplete="email" autoCapitalize="none" spellCheck={false} value={email} onChange={e => setEmail(limparEmail(e.target.value))} required />
      </label>
      <label className="field">Senha
        <div className="input-group">
          <input className="input" type={mostrar ? 'text' : 'password'} autoComplete="current-password" value={senha} onChange={e => setSenha(e.target.value)} required />
          <button type="button" className="addon" onClick={() => setMostrar(!mostrar)}>{mostrar ? 'Ocultar' : 'Ver'}</button>
        </div>
      </label>
      {erro && <div className="alert" role="alert">{erro}</div>}
      <button className="btn full" type="submit" disabled={ocupado}>{ocupado ? 'Entrando...' : 'Entrar'}</button>
    </form>
    <p className="auth-bottom">Ainda não tem conta? <Link className="text-button" to="/cadastro-adulto">Cadastre-se como responsável</Link></p>
  </section></main>;
}
