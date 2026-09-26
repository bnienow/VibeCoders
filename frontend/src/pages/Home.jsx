import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useCantina } from '../hooks/useCantina';
import { dinheiro } from '../services/catalogo';

export default function Home() {
  const { usuario } = useAuth();
  const { estado } = useCantina();
  const pendente = estado.pedidos.find(pedido => pedido.usuarioId === usuario.id && pedido.status === 'Confirmado');
  return <div className="section">
    <section className="hero-panel"><div>
      <span className="badge">☀ Bom dia, {usuario.nome.split(' ')[0]}!</span>
      <h1 style={{marginTop:18}}>Seu intervalo merece um lanche caprichado.</h1>
      <p>Confira o cardápio de hoje, escolha seus favoritos e deixe tudo pronto para retirar.</p>
      <div className="row wrap"><Link className="btn" to="/cardapio">➜ Ver cardápio</Link><span className="small">◷ Retirada no intervalo</span></div>
    </div><div className="image-slot" role="img" aria-label="Espaço reservado para imagem do lanche" /></section>
    {pendente && <section className="success" style={{marginTop:22}}>Pedido {pendente.id} confirmado. Código de retirada: <strong>{pendente.codigoRetirada}</strong>. <Link className="text-button" to="/historico">Ver pedido</Link></section>}
    <div className="grid-3" style={{marginTop:28}}>
      <article className="surface feature"><strong>◷ Sem fila</strong><p className="muted small">Peça antes do intervalo e retire no balcão.</p></article>
      <article className="surface feature"><strong>♧ Feito hoje</strong><p className="muted small">Ingredientes frescos e receitas preparadas na escola.</p></article>
      <article className="surface feature"><strong>♢ Compra segura</strong><p className="muted small">Saldo e histórico sempre visíveis para você.</p></article>
    </div>
    <section className="about"><div className="image-slot" role="img" aria-label="Espaço reservado para imagem da cantina" /><div>
      <span className="eyebrow">Sobre a cantina</span><h2>Receitas simples, ingredientes honestos e muito cuidado.</h2>
      <p className="muted">A Cantina do Pátio prepara opções para o intervalo. Confira os itens, faça seu pedido e acompanhe tudo pelo histórico.</p>
      <p className="green strong">✓ Produção diária{'\u3000'}✓ Opções variadas{'\u3000'}✓ Atendimento próximo</p>
    </div></section>
    <div className="grid-3" style={{marginTop:32}}>
      <Link className="btn" to="/cardapio">Fazer pedido</Link>
      <Link className="btn secondary" to="/historico">Meus pedidos</Link>
      <Link className="btn secondary" to="/perfil">Meu saldo: {dinheiro(usuario.saldo)}</Link>
    </div>
  </div>;
}
