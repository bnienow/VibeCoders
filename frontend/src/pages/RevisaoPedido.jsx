import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import Quantidade from '../components/Quantidade';
import { useAuth } from '../hooks/useAuth';
import { useCantina } from '../hooks/useCantina';
import { dinheiro, intervaloAberto, intervalos } from '../services/catalogo';

export default function RevisaoPedido() {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const { estado, itensCarrinho, totalCarrinho, service } = useCantina();
  const [pagamento, setPagamento] = useState('Conta'), [erro, setErro] = useState(''), [ocupado, setOcupado] = useState(false);
  function confirmar() {
    setErro(''); setOcupado(true);
    try { const pedido = service.confirmarPedido(pagamento); navigate('/historico', { state: { aviso: `Pedido ${pedido.id} confirmado. Código: ${pedido.codigoRetirada}` } }); }
    catch (falha) { setErro(falha.message); } finally { setOcupado(false); }
  }
  return <div className="container section" style={{maxWidth:1000}}>
    <span className="eyebrow">Revise antes de finalizar</span><h1>Seu pedido está quase pronto</h1><p className="muted">Confira os itens, ajuste se necessário e escolha como prefere pagar.</p>
    <div className="order-layout" style={{marginTop:25}}><section className="surface pad">
      <div className="between"><div><strong className="green">CANTINA DO PÁTIO</strong><p className="muted small">Retirada no intervalo da {intervalos[estado.intervalo].nome.toLowerCase()}</p></div><span className="badge amber">AGUARDANDO CONFIRMAÇÃO</span></div><div className="divider" />
      {itensCarrinho.length ? itensCarrinho.map(item => <div className="order-line" key={item.id}><div><strong>{item.nome}</strong><div className="muted small">{dinheiro(item.preco)} por unidade</div></div><Quantidade nome={item.nome} valor={item.quantidade} maximo={item.estoque} aoAlterar={valor => service.definirQuantidade(item.id,valor)} /><strong>{dinheiro(item.quantidade*item.preco)}</strong></div>) : <div className="alert">Seu pedido está vazio. Volte ao cardápio para escolher itens.</div>}
      <div className="between total-line"><span>TOTAL</span><span className="accent">{dinheiro(totalCarrinho)}</span></div><p className="muted small right" style={{marginTop:18}}>Este resumo não é um documento fiscal • Retirada no balcão.</p>
    </section><aside className="surface pad"><h3>Pagamento</h3><p className="muted small">Use sua conta ou pague à vista no balcão.</p>
      <label className="field" style={{margin:'17px 0'}}>Forma de pagamento<select className="input" value={pagamento} onChange={e => setPagamento(e.target.value)}><option value="Conta">Lançar na minha conta</option><option value="AVista">Pagar à vista na retirada</option></select></label>
      <div className="success">Seu saldo<br/><strong style={{fontSize:23}}>{dinheiro(usuario.saldo)}</strong><div className="small">{pagamento === 'Conta' ? `Após a compra: ${dinheiro(usuario.saldo-totalCarrinho)}` : 'Pagamento à vista não altera a conta'}</div></div>
      {!intervaloAberto(estado.intervalo) && <div className="alert" style={{marginTop:12}}>Pedidos para este intervalo já fecharam.</div>}
    </aside></div>
    {erro && <div className="alert" role="alert" style={{marginTop:15}}>{erro}</div>}
    <div className="between wrap no-print" style={{marginTop:20}}><Link className="btn soft" to="/cardapio">← Voltar ao cardápio</Link><button className="btn" onClick={confirmar} disabled={!itensCarrinho.length || !intervaloAberto(estado.intervalo) || ocupado}>{ocupado?'Confirmando...':`Confirmar • ${dinheiro(totalCarrinho)}`}</button></div>
  </div>;
}
