import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useCantina } from '../hooks/useCantina';
import { dataLocal, dinheiro, intervaloAberto, intervalos } from '../services/catalogo';

export default function Historico() {
  const { usuario } = useAuth();
  const { estado, service } = useCantina();
  const aviso = useLocation().state?.aviso; // mensagem enviada por quem navegou para cá (ex.: pedido confirmado)
  const [filtro, setFiltro] = useState('todos'), [selecionado, setSelecionado] = useState(null), [mensagem, setMensagem] = useState(aviso || '');
  const pedidos = estado.pedidos.filter(pedido => pedido.usuarioId === usuario.id && (filtro === 'mes' ? pedido.data.slice(0,7) === dataLocal().slice(0,7) : filtro === 'hoje' ? pedido.data === dataLocal() : true));
  const consumido = pedidos.filter(pedido => pedido.status !== 'Cancelado').reduce((soma,pedido) => soma + pedido.total,0);
  function cancelar(id) {
    try { service.cancelarPedido(id); setMensagem('Pedido cancelado e valor da conta estornado, quando aplicável.'); }
    catch (falha) { setMensagem(falha.message); }
  }
  return <div className="container section">
    <span className="eyebrow">Seus pedidos</span><div className="between wrap"><div><h1>Tudo o que você já pediu</h1><p className="muted">Consulte valores, itens e o status de cada compra feita na cantina.</p></div><div className="row no-print"><select className="input" aria-label="Filtrar pedidos" value={filtro} onChange={e=>setFiltro(e.target.value)}><option value="todos">Todos os pedidos</option><option value="mes">Este mês</option><option value="hoje">Hoje</option></select><button className="btn secondary" onClick={() => window.print()}>Imprimir / salvar PDF</button></div></div>
    {mensagem && <div className="success" role="status" style={{marginTop:20}}>{mensagem} <button className="text-button" onClick={() => setMensagem('')}>Fechar</button></div>}
    <div className="grid-3" style={{marginTop:24}}><article className="surface pad"><strong>{pedidos.length}</strong><p className="muted small">pedidos no filtro</p></article><article className="surface pad"><strong>{dinheiro(consumido)}</strong><p className="muted small">total dos pedidos</p></article><article className="surface pad"><strong>{dinheiro(usuario.saldo)}</strong><p className="muted small">saldo atual</p></article></div>
    <div style={{marginTop:26}}>{!pedidos.length && <div className="surface pad">Nenhum pedido encontrado. <Link className="text-button" to="/cardapio">Ver cardápio</Link></div>}
      {pedidos.map(pedido => <article className="surface history-row" key={pedido.id}>
        <div><strong>#{pedido.id}</strong><div className="muted small">{new Date(`${pedido.data}T12:00:00`).toLocaleDateString('pt-BR')} • {intervalos[pedido.intervalo]?.nome || 'Balcão'}</div></div>
        <div className="small">{pedido.itens.map(item => `${item.quantidade}× ${item.nome}`).join(' • ')}</div>
        <span className={`badge ${pedido.status === 'Cancelado' ? 'red' : ''}`}>{pedido.status}</span>
        <div className="right"><strong>{dinheiro(pedido.total)}</strong><div><button className="text-button" onClick={() => setSelecionado(selecionado === pedido.id ? null : pedido.id)}>Detalhes</button></div></div>
        {selecionado === pedido.id && <div style={{gridColumn:'1 / -1',borderTop:'1px solid #eaded0',paddingTop:15}}>
          {pedido.status === 'Confirmado' && <p className="green strong">Código para retirada: <span style={{fontSize:24}}>{pedido.codigoRetirada}</span></p>}
          {pedido.itens.map(item => <p className="between small" key={item.id}><span>{item.quantidade}× {item.nome} • {dinheiro(item.preco)} cada</span><strong>{dinheiro(item.quantidade*item.preco)}</strong></p>)}
          <p className="muted small">Pagamento: {pedido.formaPagamento === 'Conta' ? 'Conta' : 'À vista'}</p>
          {pedido.status === 'Confirmado' && intervaloAberto(pedido.intervalo,pedido.data) && <button className="btn soft no-print" onClick={() => cancelar(pedido.id)}>Cancelar pedido</button>}
        </div>}
      </article>)}
    </div>
  </div>;
}
