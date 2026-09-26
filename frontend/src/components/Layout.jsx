import Tema from './Tema';
import Header from './Header';
import Footer from './Footer';
export default function Layout({ perfil, nome, saldo, children }) {
  return <><Tema/><div className="shell"><Header perfil={perfil} nome={nome} saldo={saldo}/><main className="main"><div className="container">{children}</div></main><Footer/></div></>;
}