import Layout from './Layout';
import Footer from './Footer';
import { useCantina } from '../hooks/useCantina';
// Compatibilidade com componentes antigos; sem navegação ou destino Home.
export const Rodape = Footer;
export default function Estrutura({ children, perfil = 'Aluno' }) {
  const { usuario } = useCantina();
  return <Layout perfil={perfil} nome={usuario?.nome} saldo={usuario?.saldo}>{children}</Layout>;
}