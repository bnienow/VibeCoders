import { useState } from 'react';
import Estrutura from '../components/Estrutura';
import { useCantina } from '../hooks/useCantina';
import { dinheiro } from '../services/catalogo';

export default function Perfil({ navigate }) {
  const { usuario, estado, service } = useCantina();
  const [editando, setEditando] = useState(false), [nome, setNome] = useState(usuario.nome), [email, setEmail] = useState(usuario.email);
  const [avisos, setAvisos] = useState(usuario.avisos !== false), [mensagem, setMensagem] = useState(''), [erro, setErro] = useState('');
  const ultimoMovimento = estado.movimentos.find(movimento => movimento.usuarioId === usuario.id);
  function salvar(evento) {
    evento.preventDefault(); setErro(''); setMensagem('');
    try { service.atualizarPerfil({nome,email,avisos}); setEditando(false); setMensagem('Dados atualizados.'); }
    catch (falha) { setErro(falha.message); }
  }
  function sair() { service.sair(); navigate('/login'); }
  return <Estrutura pagina="/perfil" navigate={navigate}><div className="container section">
    <span className="eyebrow">Minha conta</span><h1>Seu perfil e saldo em um só lugar</h1><p className="muted">Confira seus dados, acompanhe o saldo e mantenha sua conta atualizada.</p>
    {mensagem && <div className="success" role="status" style={{marginTop:18}}>{mensagem}</div>}{erro && <div className="alert" role="alert" style={{marginTop:18}}>{erro}</div>}
    <div className="profile-grid" style={{marginTop:24}}>
      <section className="surface profile-card" style={{textAlign:'center'}}><div className="image-slot portrait" role="img" aria-label="Espaço reservado para foto de perfil" /><h2 style={{marginBottom:8}}>{usuario.nome}</h2><span className="badge">Aluno • {usuario.turma}</span><div className="divider" /><div className="between small"><span className="muted">Matrícula</span><strong>{usuario.id === 'aluno-demo' ? '2026.0842' : usuario.id.slice(0,8)}</strong></div><div className="between small" style={{marginTop:10}}><span className="muted">Responsável</span><strong>Vinculado</strong></div></section>
      <section className="surface profile-card"><div className="between"><div><h2 style={{marginBottom:3}}>Dados básicos</h2><p className="muted small">Toque em editar para atualizar.</p></div>{!editando && <button className="btn secondary" onClick={() => setEditando(true)}>Editar</button>}</div><div className="divider" />
        {editando ? <form className="stack" onSubmit={salvar}><label className="field">Nome completo<input className="input" value={nome} onChange={e=>setNome(e.target.value)} required /></label><label className="field">E-mail institucional<input className="input" type="email" value={email} onChange={e=>setEmail(e.target.value)} required /></label><label className="row small"><input type="checkbox" checked={avisos} onChange={e=>setAvisos(e.target.checked)} /> Avisos de pedidos</label><div className="row"><button className="btn" type="submit">Salvar alterações</button><button className="btn secondary" type="button" onClick={() => setEditando(false)}>Cancelar</button></div></form> : <div className="stack"><div className="between"><span className="muted small">Nome completo</span><strong>{usuario.nome}</strong></div><div className="divider" style={{margin:0}} /><div className="between"><span className="muted small">E-mail</span><strong className="small">{usuario.email}</strong></div><div className="divider" style={{margin:0}} /><div className="between"><span className="muted small">Avisos de pedidos</span><strong>{usuario.avisos === false ? 'Desligados' : 'Ligados'}</strong></div></div>}
      </section>
      <div className="stack"><section className="surface profile-card dark"><strong className="small">◧ Saldo disponível</strong><h1 style={{marginTop:13}}>{dinheiro(usuario.saldo)}</h1><p className="muted small">{usuario.saldo < 0 ? `${dinheiro(Math.abs(usuario.saldo))} de R$ 250,00 em fiado` : 'Crédito disponível na conta'}</p><button className="btn full" type="button" onClick={() => setMensagem('Somente o responsável pode adicionar crédito à conta do aluno.')}>+ Solicitar recarga</button></section>
      <section className="surface profile-card"><strong className="small">Último movimento</strong><p className="green strong" style={{margin:'12px 0 4px'}}>{ultimoMovimento ? dinheiro(ultimoMovimento.valor) : 'Nenhum movimento'}</p><p className="muted small">{ultimoMovimento?.descricao || 'Sua conta está pronta para uso.'}</p><button className="text-button" onClick={() => navigate('/historico')}>Ver histórico</button></section><button className="btn soft full" onClick={sair}>Sair da conta</button></div>
    </div>
  </div></Estrutura>;
}
