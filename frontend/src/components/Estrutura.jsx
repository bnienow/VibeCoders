import { dinheiro } from '../services/catalogo';
import { useCantina } from '../hooks/useCantina';

export function Rodape() {
  return <footer className="site-footer"><div className="container footer-grid">
    <div><strong>Cantina do Pátio</strong><p>Comida gostosa, cuidado de verdade e mais energia para aprender.</p></div>
    <div><strong>Fale com a cantina</strong><p>Atendimento no pátio central • Bloco B</p></div>
    <div><strong>Horários</strong><p>Seg–Sex • 7h às 17h30</p></div>
  </div></footer>;
}

export default function Estrutura({ pagina, navigate, children }) {
  const { usuario } = useCantina();
  const links = [['Home','/home'],['Cardápio','/cardapio'],['Histórico','/historico'],['Catálogo','/catalogo']];
  return <div className="page-shell">
    <header className="site-header"><div className="container header-inner">
      <button className="brand" type="button" onClick={() => navigate('/home')}><span className="brand-mark">♜</span><span>Cantina<small>DO PÁTIO</small></span></button>
      <div className="header-person"><strong>{usuario?.nome}</strong><span>Aluno • {usuario?.turma}</span></div>
      <nav className="site-nav" aria-label="Navegação principal">{links.map(([texto, rota]) => <button key={rota} type="button" className={pagina === rota ? 'active' : ''} onClick={() => navigate(rota)}>{texto}</button>)}</nav>
      <button type="button" className="balance-pill" onClick={() => navigate('/perfil')} title="Ver meu saldo">◧ {dinheiro(usuario?.saldo ?? 0)}</button>
      <button type="button" className="btn secondary" style={{padding:'7px 10px',minHeight:33}} onClick={() => navigate('/perfil')} aria-label="Abrir perfil">♙</button>
    </div></header>
    <main style={{flex:1}}>{children}</main><Rodape />
  </div>;
}