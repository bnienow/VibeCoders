import Estrutura from '../components/Estrutura';
import { useCantina } from '../hooks/useCantina';
import { dinheiro } from '../services/catalogo';

export default function Home({ navigate }) {
  const { usuario, estado } = useCantina();
  const pendente = estado.pedidos.find(pedido => pedido.usuarioId === usuario.id && pedido.status === 'Confirmado');
  return <Estrutura pagina="/home" navigate={navigate}><div className="container section">
    <section className="hero-panel"><div>
      <span className="badge">☀ Bom dia, {usuario.nome.split(' ')[0]}!</span>
      <h1 style={{marginTop:18}}>Seu intervalo merece um lanche caprichado.</h1>
      <p>Confira o cardápio de hoje, escolha seus favoritos e deixe tudo pronto para retirar.</p>
      <div className="row wrap"><button className="btn" onClick={() => navigate('/cardapio')}>➜ Ver cardápio</button><span className="small">◷ Retirada no intervalo</span></div>
    </div><div className="image-slot" role="img" aria-label="Espaço reservado para imagem do lanche" /></section>
    {pendente && <section className="success" style={{marginTop:22}}>Pedido {pendente.id} confirmado. Código de retirada: <strong>{pendente.codigoRetirada}</strong>. <button className="text-button" onClick={() => navigate('/historico')}>Ver pedido</button></section>}
    <div className="grid-3" style={{marginTop:28}}>
      <article className="surface feature"><strong>◷ Sem fila</strong><p className="muted small">Peça antes do intervalo e retire no balcão.</p></article>
      <article className="surface feature"><strong>♧ Feito hoje</strong><p className="muted small">Ingredientes frescos e receitas preparadas na escola.</p></article>
      <article className="surface feature"><strong>♢ Compra segura</strong><p className="muted small">Saldo e histórico sempre visíveis para você.</p></article>
    </div>
    <section className="about"><div className="image-slot" role="img" aria-label="Espaço reservado para imagem da cantina" /><div>
      <span className="eyebrow">Sobre a cantina</span><h2>Receitas simples, ingredientes honestos e muito cuidado.</h2>
      <p className="muted">A Cantina do Pátio prepara opções para o intervalo. Confira os itens, faça seu pedido e acompanhe tudo pelo histórico.</p>
      <p className="green strong">✓ Produção diária　✓ Opções variadas　✓ Atendimento próximo</p>
    </div></section>
    <div className="grid-3" style={{marginTop:32}}>
      <button className="btn" onClick={() => navigate('/cardapio')}>Fazer pedido</button>
      <button className="btn secondary" onClick={() => navigate('/historico')}>Meus pedidos</button>
      <button className="btn secondary" onClick={() => navigate('/perfil')}>Meu saldo: {dinheiro(usuario.saldo)}</button>
    </div>
  </div></Estrutura>;
}
