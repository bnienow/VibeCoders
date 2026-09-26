import { Link, Navigate, NavLink, Outlet } from 'react-router-dom';
import { dinheiro } from '../services/catalogo';
import { useAuth } from '../hooks/useAuth';

const links = [['Home', '/home'], ['Cardápio', '/cardapio'], ['Histórico', '/historico'], ['Catálogo', '/catalogo']];

export function Rodape() {
  return <footer className="site-footer"><div className="container footer-grid">
    <div><strong>Cantina do Pátio</strong><p>Comida gostosa, cuidado de verdade e mais energia para aprender.</p></div>
    <div><strong>Fale com a cantina</strong><p>Atendimento no pátio central • Bloco B</p></div>
    <div><strong>Horários</strong><p>Seg–Sex • 7h às 17h30</p></div>
  </div></footer>;
}

// Layout da área logada: sem usuário, manda para o login; com usuário, cabeçalho + página (Outlet) + rodapé.
// NavLink marca sozinho o link da página atual com a classe "active".
export default function Estrutura() {
  const { usuario, carregando } = useAuth();
  if (carregando) return null; // ainda perguntando à API se há sessão
  if (!usuario) return <Navigate to="/login" replace />;

  return <div className="page-shell">
    <header className="site-header"><div className="container header-inner">
      <Link className="brand" to="/home"><span className="brand-mark">♜</span><span>Cantina<small>DO PÁTIO</small></span></Link>
      <div className="header-person"><strong>{usuario.nome}</strong><span>{usuario.permissao}</span></div>
      <nav className="site-nav" aria-label="Navegação principal">{links.map(([texto, rota]) => <NavLink key={rota} to={rota}>{texto}</NavLink>)}</nav>
      {usuario.saldo != null && <Link className="balance-pill" to="/perfil" title="Ver meu saldo">◧ {dinheiro(usuario.saldo)}</Link>}
      <Link className="btn secondary" style={{padding:'7px 10px',minHeight:33}} to="/perfil" aria-label="Abrir perfil">♙</Link>
    </div></header>
    <main style={{flex:1}}><Outlet /></main><Rodape />
  </div>;
}
