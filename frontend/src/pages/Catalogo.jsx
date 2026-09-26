import { useState } from 'react';
import { Link } from 'react-router-dom';
import Quantidade from '../components/Quantidade';
import { useCantina } from '../hooks/useCantina';
import { dinheiro } from '../services/catalogo';

const categorias = ['Salgados','Doces','Bebidas','Combos'];
export default function Catalogo() {
  const { estado, itensCarrinho, totalCarrinho, service } = useCantina();
  const [busca, setBusca] = useState('');
  const disponiveis = estado.itens.filter(item => item.ativo && item.disponivel && item.estoque > 0 && `${item.nome} ${item.descricao}`.toLowerCase().includes(busca.toLowerCase()));
  return <div className="container section">
    <span className="eyebrow">Nosso cardápio</span><div className="between wrap"><div><h1>Conheça tudo o que preparamos</h1><p className="muted">Veja os itens, ingredientes descritos e preços. Adicione favoritos ao seu pedido.</p></div><Link className="btn secondary" to="/cardapio">{itensCarrinho.length} itens • {dinheiro(totalCarrinho)}</Link></div>
    <label className="field" style={{maxWidth:360,marginTop:20}}>Buscar no catálogo<input className="input" value={busca} onChange={e=>setBusca(e.target.value)} placeholder="Nome ou descrição" /></label>
    {categorias.map(categoria => { const grupo = disponiveis.filter(item => item.categoria === categoria); if (!grupo.length) return null;
      return <section className="surface category" key={categoria}><div className="between category-heading"><h2 style={{margin:0}}>{categoria}</h2><span className="muted small">{grupo.length} alimentos</span></div><div className="catalog-grid">{grupo.map(item => <article className="surface catalog-card" key={item.id}><div><h3>{item.nome}</h3><p className="muted">{item.descricao}</p>{item.alergenos.length > 0 && <p className="muted">Alérgenos: {item.alergenos.join(', ')}</p>}<strong className="green">{dinheiro(item.preco)}</strong></div><Quantidade nome={item.nome} valor={estado.carrinho[item.id] || 0} maximo={item.estoque} aoAlterar={valor => service.definirQuantidade(item.id,valor)} /></article>)}</div></section>;
    })}
    {!disponiveis.length && <div className="surface pad" style={{marginTop:20}}>Nenhum item encontrado.</div>}
    {itensCarrinho.length > 0 && <div className="right no-print" style={{marginTop:22}}><Link className="btn" to="/cardapio">Escolher intervalo e continuar • {dinheiro(totalCarrinho)}</Link></div>}
  </div>;
}
