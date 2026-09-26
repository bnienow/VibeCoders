import { useState } from 'react';
import Feedback from '../components/Feedback';
import Quantidade from '../components/Quantidade';
import { useApi } from '../hooks/useApi';
import { dataBR, dataISO, dinheiro } from '../services/formatos';
import { itemService } from '../services/itemService';
import { pedidoService } from '../services/pedidoService';

const COR_STATUS = { Cancelado: 'red', Confirmado: 'amber' };

export default function Historico() {
  const { dados: todos, recarregar } = useApi(() => pedidoService.meus(), []);
  const [filtro, setFiltro] = useState('Todos'), [feedback, setFeedback] = useState({});
  const [editando, setEditando] = useState(null), [opcoes, setOpcoes] = useState([]), [quantidades, setQuantidades] = useState({});

  const hoje = dataISO();
  const pedidos = (todos ?? []).filter(p =>
    filtro === 'Todos' || (filtro === 'Hoje' && p.data === hoje) || (filtro === 'Mês' && p.data.slice(0, 7) === hoje.slice(0, 7)));

  // Abre a edição: carrega o cardápio daquele dia/intervalo e parte das quantidades atuais
  async function comecarEdicao(p) {
    setFeedback({});
    setQuantidades(Object.fromEntries(p.itens.map(i => [i.itemId, i.quantidade])));
    setOpcoes(await itemService.cardapio(p.data, p.intervaloId));
    setEditando(p.id);
  }

  // Executa a ação na API, mostra o resultado e recarrega a lista
  async function executar(acao, mensagem) {
    try {
      await acao();
      setEditando(null);
      setFeedback({ sucesso: mensagem });
      recarregar();
    } catch (e) {
      setFeedback({ erro: e.message });
    }
  }

  const itensEditados = () => Object.entries(quantidades)
    .filter(([, quantidade]) => quantidade > 0)
    .map(([itemId, quantidade]) => ({ itemId: Number(itemId), quantidade }));

  return <>
    <div className="section-head"><h1>Meus pedidos</h1></div>
    <Feedback {...feedback}/>
    <div className="pill-tabs" style={{ margin: '16px 0 20px' }}>{['Todos', 'Hoje', 'Mês'].map(v => <button key={v} aria-pressed={filtro === v} onClick={() => setFiltro(v)}>{v}</button>)}</div>

    <div className="stack">{pedidos.map(p => <article className="surface pad" key={p.id}>
      <div className="between wrap">
        <div className="stack" style={{ gap: 6 }}>
          <span className="small muted">{dataBR(p.data)} · {p.intervalo || 'Balcão'} · {p.formaPagamento === 'AVista' ? 'à vista' : 'na conta'}</span>
          <div className="row"><h3>Código {p.codigoRetirada}</h3><span className={`badge ${COR_STATUS[p.status] ?? ''}`}>{p.status}</span></div>
        </div>
        <div className="metric">{dinheiro(p.total)}</div>
      </div>
      <div className="divider"/>
      {p.itens.map(i => <div className="line" key={i.itemId}><span>{i.quantidade} × {i.nome}</span><span>{dinheiro(i.subtotal)}</span></div>)}

      {p.podeAlterar && editando !== p.id && <div className="row wrap no-print" style={{ marginTop: 16 }}>
        <button className="btn secondary small" onClick={() => comecarEdicao(p)}>Alterar</button>
        <button className="btn danger small" onClick={() => executar(() => pedidoService.cancelar(p.id), 'Pedido cancelado e valor estornado na conta.')}>Cancelar</button>
      </div>}

      {editando === p.id && <div className="surface pad" style={{ marginTop: 16, background: '#f8fbf8' }}>
        <h3 style={{ marginBottom: 8 }}>Alterar itens</h3>
        {opcoes.map(i => <div className="line" key={i.id}>
          <span>{i.nome} · {dinheiro(i.precoUnitario)}</span>
          <Quantidade nome={i.nome} valor={quantidades[i.id] || 0} maximo={i.estoque + (p.itens.find(l => l.itemId === i.id)?.quantidade || 0)} aoMudar={n => setQuantidades({ ...quantidades, [i.id]: n })}/>
        </div>)}
        <div className="row" style={{ marginTop: 14 }}>
          <button className="btn small" onClick={() => executar(() => pedidoService.alterar(p.id, itensEditados()), 'Pedido alterado.')}>Salvar alterações</button>
          <button className="btn secondary small" onClick={() => setEditando(null)}>Fechar</button>
        </div>
      </div>}
    </article>)}
    {todos && !pedidos.length && <div className="empty">Nenhum pedido no período.</div>}</div>
  </>;
}
