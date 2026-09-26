import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Quantidade from '../components/Quantidade';
import { useAuth } from '../hooks/useAuth';
import { useCantina } from '../hooks/useCantina';
import { dinheiro, intervaloAberto, intervalos, minutosRestantes } from '../services/catalogo';

const categorias = ['Salgados','Doces','Bebidas','Combos'];
export default function Cardapio() {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const { estado, itensCarrinho, totalCarrinho, service } = useCantina();
  const [busca, setBusca] = useState(''), [erro, setErro] = useState('');
  const aberto = intervaloAberto(estado.intervalo);
  const itens = estado.itens.filter(item => item.ativo && item.disponivel && item.estoque > 0 && item.nome.toLowerCase().includes(busca.toLowerCase()));
  function alergia(item) {
    const restricoes = (usuario.restricoes || '').toLowerCase().split(',').map(texto => texto.trim());
    return item.alergenos.find(alergeno => restricoes.includes(alergeno.toLowerCase()));
  }
  function revisar() {
    setErro('');
    if (!aberto) { setErro('Pedidos para este intervalo já fecharam.'); return; }
    if (!itensCarrinho.length) { setErro('Selecione pelo menos um item.'); return; }
    navigate('/revisao');
  }
  return <div className="section">
    <span className="eyebrow">Cardápio de hoje</span><div className="between wrap"><div><h1>Escolha o que vai deixar seu intervalo melhor</h1><p className="muted">Ajuste as quantidades. Você confere tudo antes de finalizar.</p></div><span className="badge">{itensCarrinho.reduce((soma,item) => soma + item.quantidade,0)} itens selecionados</span></div>
    <div className="surface pad grid-2" style={{marginTop:20}}>
      <label className="field">Intervalo para retirada<select className="input" value={estado.intervalo} onChange={e => service.escolherIntervalo(e.target.value)}>{Object.entries(intervalos).map(([chave, intervalo]) => <option key={chave} value={chave}>{intervalo.nome} • {intervalo.inicio}</option>)}</select></label>
      <label className="field">Buscar item<input className="input" value={busca} onChange={e => setBusca(e.target.value)} placeholder="Nome do lanche" /></label>
      <p className={aberto?'green small':'accent small'}>{aberto ? `Pedidos fecham às ${intervalos[estado.intervalo].fechamento} (em aproximadamente ${minutosRestantes(estado.intervalo)} min).` : `Pedidos para ${intervalos[estado.intervalo].nome.toLowerCase()} já fecharam. O balcão continua disponível.`}</p>
      <p className="muted small right">Saldo projetado ao pagar na conta: {dinheiro(usuario.saldo - totalCarrinho)}</p>
    </div>
    {categorias.map(categoria => { const grupo = itens.filter(item => item.categoria === categoria); if (!grupo.length) return null;
      return <section className="surface category" key={categoria}><div className="between category-heading"><h2 style={{margin:0}}>{categoria}</h2><span className="muted small">{grupo.length} opções</span></div><div className="product-grid">{grupo.map(item => { const quantidade = estado.carrinho[item.id] || 0, conflito = alergia(item);
        return <article className={`product ${quantidade?'chosen':''}`} key={item.id}><div className="between"><h3>{item.nome}</h3><Quantidade nome={item.nome} valor={quantidade} maximo={item.estoque} aoMudar={valor => service.definirQuantidade(item.id,valor)} /></div><p>{item.descricao}</p>{conflito && <span className="badge red">Contém {conflito}</span>}<div className="product-price">{dinheiro(item.preco)}</div></article>;
      })}</div></section>;
    })}
    {!itens.length && <div className="surface pad">Nenhum item disponível para essa busca.</div>}
    <div className="between wrap" style={{marginTop:24}}><p className="muted small">Itens sujeitos à disponibilidade no momento da confirmação.</p><div className="row"><strong>Total: {dinheiro(totalCarrinho)}</strong><button className="btn" onClick={revisar} disabled={!itensCarrinho.length || !aberto}>➜ Revisar pedido</button></div></div>
    {erro && <div className="alert" role="alert">{erro}</div>}
  </div>;
}
