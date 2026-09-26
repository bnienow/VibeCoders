import { Link, Navigate, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { MENU, PAGINA_INICIAL } from '../rotas';

// Layout da área logada. Sem sessão → login; logado em outro perfil → página inicial dele.
// Com acesso: cabeçalho com o menu do perfil + página (Outlet).
// NavLink marca o link da página atual com a classe "active" ("end" = só o endereço exato).
export default function Estrutura({ permissoes }) {
  const { usuario, carregando, sair } = useAuth();
  const navigate = useNavigate();

  if (carregando) return null; // ainda perguntando à API se há sessão
  if (!usuario) return <Navigate to="/login" replace />;
  if (!permissoes.includes(usuario.permissao)) return <Navigate to={PAGINA_INICIAL[usuario.permissao]} replace />;

  async function encerrarSessao() {
    await sair();
    navigate('/login');
  }

  return <div className="shell">
    <header className="page-header"><div className="container header-inner">
      <div className="row"><Link className="logo" to={PAGINA_INICIAL[usuario.permissao]}>Cantina<span> do Pátio</span></Link><span className="role">{usuario.permissao}</span></div>
      <nav className="site-nav" aria-label="Navegação principal">{MENU[usuario.permissao].map(([texto, rota]) => <NavLink key={rota} to={rota} end>{texto}</NavLink>)}</nav>
      <div className="header-actions"><strong className="small">{usuario.nome}</strong><button type="button" className="btn secondary" onClick={encerrarSessao}>Sair</button></div>
    </div></header>
    <main className="main"><div className="container"><Outlet /></div></main>
  </div>;
}
