import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import ProdutoCard from '../components/ProdutoCard';
import { useApi } from '../hooks/useApi';
import { useAuth } from '../hooks/useAuth';
import { useCarrinho } from '../hooks/useCarrinho';
import { alunoService } from '../services/alunoService';
import { CATEGORIAS, dataISO, dinheiro, hora, janelaAberta } from '../services/formatos';
import { intervaloService } from '../services/intervaloService';
import { itemService } from '../services/itemService';

export default function Cardapio() {
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const carrinho = useCarrinho();
  const [busca, setBusca] = useState('');

  const { dados: intervalos } = useApi(() => intervaloService.listar(), []);
  const { dados: aluno } = useApi(() => alunoService.detalhe(usuario.id), [usuario.id]);

  // Sem intervalo escolhido ainda, mostra o primeiro (Manhã)
  const intervalo = intervalos?.find(i => i.id === carrinho.intervaloId) ?? intervalos?.[0];
  const { dados: cardapio } = useApi(
    () => intervalo ? itemService.cardapio(carrinho.data, intervalo.id) : Promise.resolve([]),
    [carrinho.data, intervalo?.id]);

  const aberto = janelaAberta(intervalo, carrinho.data);
  const itens = (cardapio ?? []).filter(item => item.nome.toLowerCase().includes(busca.toLowerCase()));
  const totalDeItens = carrinho.itens.reduce((soma, l) => soma + l.quantidade, 0);

  function revisar() {
    carrinho.escolher(carrinho.data, intervalo.id);
    navigate('/revisao');
  }

  return <>
    <div className="section-head"><h1>Cardápio</h1></div>

    <div className="surface pad toolbar" style={{ marginBottom: 12 }}>
      <label className="field">Dia da retirada
        <select className="input" value={carrinho.data} onChange={e => carrinho.escolher(e.target.value, intervalo?.id)}>
          <option value={dataISO(0)}>Hoje</option>
          <option value={dataISO(1)}>Amanhã</option>
        </select>
      </label>
      <label className="field">Intervalo
        <select className="input" value={intervalo?.id ?? ''} onChange={e => carrinho.escolher(carrinho.data, Number(e.target.value))}>
          {intervalos?.map(i => <option key={i.id} value={i.id}>{i.nome} · {hora(i.horaInicio)}</option>)}
        </select>
      </label>
      <label className="field">Buscar item
        <input className="input" type="search" value={busca} onChange={e => setBusca(e.target.value)} />
      </label>
    </div>
    {intervalo && <p className={aberto ? 'small muted' : 'alert'}>
      {aberto ? `Pedidos para este intervalo fecham às ${hora(intervalo.fechamento)}.` : `Pedidos para ${intervalo.nome.toLowerCase()} já fecharam. O balcão continua disponível.`}
    </p>}

    {Object.entries(CATEGORIAS).map(([categoria, titulo]) => {
      const grupo = itens.filter(item => item.categoria === categoria);
      if (!grupo.length) return null;
      return <section className="category" key={categoria}>
        <h2>{titulo}</h2>
        <div className="cards">{grupo.map(item =>
          <ProdutoCard key={item.id} item={item} quantidade={carrinho.quantidade(item.id)} restricoes={aluno?.restricoes} aoMudar={valor => carrinho.definirQuantidade(item, valor)} />)}
        </div>
      </section>;
    })}
    {cardapio && !itens.length && <div className="empty">Nenhum item disponível.</div>}

    <div className="surface pad between wrap" style={{ position: 'sticky', bottom: 16 }}>
      <div><strong>{totalDeItens} item(ns) · {dinheiro(carrinho.total)}</strong><div className="small muted">Saldo após o pedido: {aluno ? dinheiro(aluno.saldo - carrinho.total) : '…'}</div></div>
      <button className="btn" onClick={revisar} disabled={!carrinho.itens.length || !aberto}>Revisar pedido</button>
    </div>
  </>;
}
