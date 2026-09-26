import { useState } from 'react';
import Tema from '../components/Tema';
import Feedback from '../components/Feedback';
import { useCantina } from '../hooks/useCantina';
export default function Login({ onEntrar }) {
  const { service } = useCantina();
  const [email,setEmail]=useState(''), [senha,setSenha]=useState(''), [feedback,setFeedback]=useState({});
  function submit(e) { e.preventDefault(); try { const u=service.entrar(email,senha); setFeedback({sucesso:'Sessão demonstrativa: '+u.nome+' · '+u.papel+'.'}); onEntrar?.(u); } catch(err) { setFeedback({erro:err.message}); } }
  return <><Tema/><main className="auth"><section className="surface auth-card"><span className="eyebrow">Cantina do Pátio</span><h1>Entre na sua conta</h1><p className="muted">Acesse com dados demonstrativos. A autenticação real será conectada depois.</p><Feedback {...feedback}/><form onSubmit={submit}><label className="field">E-mail<input className="input" type="email" required value={email} onChange={(e)=>setEmail(e.target.value)} autoComplete="email"/></label><label className="field">Senha<input className="input" type="password" required value={senha} onChange={(e)=>setSenha(e.target.value)} autoComplete="current-password"/></label><button className="btn full">Entrar</button></form><p className="hint" style={{marginTop:20}}>Prévia: marina@aluno.cantina.test, carla@exemplo.test ou cantina@exemplo.test; senha123. O componente comunica o perfil pelo callback onEntrar.</p></section></main></>;
}
