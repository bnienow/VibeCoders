import { useState } from 'react';
import Feedback from '../components/Feedback';
import Quantidade from '../components/Quantidade';
import { useApi } from '../hooks/useApi';
import { useConexao } from '../hooks/useConexao';
import { alunoService } from '../services/alunoService';
import { SemConexaoError } from '../services/conexao';
import { filaOffline } from '../services/filaOffline';
import { CATEGORIAS, dinheiro } from '../services/formatos';
import { itemService } from '../services/itemService';
import { pedidoService } from '../services/pedidoService';

const TETO_FIADO = -250;
const MAXIMO_SUGESTOES = 8;

export default function PdvBalcao() {
  const { online, pendentes } = useConexao();

  // Itens e alunos guardam cópia no navegador: sem conexão, o balcão usa a última cópia (o balcão nunca fecha).
  // Recarregam quando a conexão volta e quando a fila é enviada (dependências), para atualizar estoque e saldos.
  const { dados: itens, recarregar: recarregarItens } = useApi(() => itemService.cardapio(), [online, pendentes.length], 'itens-balcao');
  const { dados: alunos, recarregar: recarregarAlunos } = useApi(() => alunoService.buscar(''), [online, pendentes.length], 'alunos-balcao');

  const [categoria, setCategoria] = useState('Salgado'), [quantidades, setQuantidades] = useState({});
  const [busca, setBusca] = useState(''), [aluno, setAluno] = useState(null), [feedback, setFeedback] = useState({});

  // Busca o comprador na lista guardada (funciona online e offline)
  const termo = busca.trim().toLowerCase();
  const encontrados = termo.length >= 2 && !aluno
    ? (alunos ?? []).filter(a => `${a.nome} ${a.email}`.toLowerCase().includes(termo)).slice(0, MAXIMO_SUGESTOES)
    : [];

  const linhas = (itens ?? []).filter(i => quantidades[i.id]);
  const total = linhas.reduce((soma, i) => soma + i.precoUnitario * quantidades[i.id], 0);
  const teto = aluno && aluno.saldo - total < TETO_FIADO;
  const alergia = [...new Set(linhas.flatMap(i => i.alergenos.filter(a => aluno?.restricoes.includes(a))))];

  const mudar = (id, n) => setQuantidades(q => ({ ...q, [id]: n }));

  function limparAtendimento() {
    setQuantidades({}); setBusca(''); setAluno(null);
  }

  // Sem conexão: a venda vai para a fila e é enviada quando a API voltar (a API reconfere as regras)
  function guardarNaFila(venda) {
    const forma = venda.formaPagamento === 'Conta' ? 'na conta' : 'à vista';
    filaOffline.adicionar(venda, `${aluno.nome} · ${dinheiro(total)} ${forma}`);
    limparAtendimento();
    setFeedback({ sucesso: 'Sem conexão: venda guardada. Ela será enviada quando a conexão voltar.' });
  }

  async function finalizar(formaPagamento) {
    const venda = { usuarioId: aluno.id, formaPagamento, itens: linhas.map(i => ({ itemId: i.id, quantidade: quantidades[i.id] })) };
    if (!online) return guardarNaFila(venda);

    try {
      const pedido = await pedidoService.venderNoBalcao(venda);
      limparAtendimento();
      setFeedback({ sucesso: `Venda ${pedido.codigoRetirada} registrada: ${dinheiro(pedido.total)} ${formaPagamento === 'Conta' ? 'na conta' : 'à vista'}.` });
      recarregarItens();
      recarregarAlunos();
    } catch (e) {
      if (e instanceof SemConexaoError) guardarNaFila(venda); // a API caiu bem na hora da venda
      else setFeedback({ erro: e.message });
    }
  }

  return <>
    <div className="section-head"><h1>Balcão</h1></div>
    <Feedback {...feedback}/>

    <div className="two-column" style={{ marginTop: 16 }}>
      <section>
        <div className="pill-tabs" style={{ marginBottom: 14 }}>{Object.entries(CATEGORIAS).map(([valor, titulo]) => <button key={valor} aria-pressed={categoria === valor} onClick={() => setCategoria(valor)}>{titulo}</button>)}</div>
        <div className="cards">{(itens ?? []).filter(i => i.categoria === categoria).map(i =>
          <button key={i.id} type="button" className={`surface pad tile ${quantidades[i.id] ? 'product chosen' : ''}`} onClick={() => mudar(i.id, Math.min(i.estoque, (quantidades[i.id] || 0) + 1))}>
            <strong>{i.nome}</strong>
            <span className="price">{dinheiro(i.precoUnitario)}</span>
            <span className="small muted">Estoque {i.estoque}{quantidades[i.id] ? ` · no pedido: ${quantidades[i.id]}` : ''}</span>
          </button>)}
        </div>
      </section>

      <aside className="surface pad sticky-panel stack">
        <label className="field">Comprador (nome ou e-mail)
          <input className="input" type="search" value={busca} onChange={e => { setBusca(e.target.value); setAluno(null); }}/>
        </label>
        {termo.length >= 2 && !aluno && <div className="stack" style={{ gap: 6 }}>
          {encontrados.map(a => <button key={a.id} className="btn secondary full small" onClick={() => { setAluno(a); setBusca(a.nome); }}>{a.nome} · {a.email}</button>)}
          {!encontrados.length && <span className="small muted">Nenhum aluno encontrado.</span>}
        </div>}
        {aluno && <div className="note">
          <strong>{aluno.nome}</strong>
          <div className="small">Saldo {dinheiro(aluno.saldo)}{online ? '' : ' (da última atualização)'} · limite diário {aluno.limiteDiario == null ? '—' : dinheiro(aluno.limiteDiario)}</div>
          <div className="small">Restrições: {aluno.restricoes.join(', ') || 'nenhuma'}</div>
        </div>}

        <div>
          {linhas.map(i => <div className="line" key={i.id}>
            <div><strong>{i.nome}</strong><div className="small muted">{dinheiro(i.precoUnitario)} cada</div></div>
            <Quantidade nome={i.nome} valor={quantidades[i.id]} maximo={i.estoque} aoMudar={n => mudar(i.id, n)}/>
          </div>)}
          {!linhas.length && <span className="small muted">Nenhum item no pedido.</span>}
        </div>

        <div className="between"><strong>Total</strong><strong className="metric">{dinheiro(total)}</strong></div>
        {alergia.length > 0 && <div className="alert">Alergia: contém {alergia.join(', ')}.</div>}
        {teto && <div className="alert">Limite de R$ 250,00 atingido — somente à vista</div>}
        <div className="grid-2" style={{ gap: 8 }}>
          <button className="btn full" disabled={!aluno || !linhas.length || teto} onClick={() => finalizar('Conta')}>Lançar na conta</button>
          <button className="btn secondary full" disabled={!aluno || !linhas.length} onClick={() => finalizar('AVista')}>Pagou à vista</button>
        </div>
      </aside>
    </div>
  </>;
}
