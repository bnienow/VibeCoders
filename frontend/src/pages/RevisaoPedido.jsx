import { useState } from 'react';
import { Link } from 'react-router-dom';
import Feedback from '../components/Feedback';
import Quantidade from '../components/Quantidade';
import { useApi } from '../hooks/useApi';
import { useAuth } from '../hooks/useAuth';
import { useCarrinho } from '../hooks/useCarrinho';
import { alunoService } from '../services/alunoService';
import { dataBR, dinheiro, hora, janelaAberta } from '../services/formatos';
import { intervaloService } from '../services/intervaloService';
import { pedidoService } from '../services/pedidoService';

const TETO_FIADO = -250;

export default function RevisaoPedido() {
  const { usuario } = useAuth();
  const carrinho = useCarrinho();
  const [feedback, setFeedback] = useState({}), [enviando, setEnviando] = useState(false);

  const { dados: intervalos } = useApi(() => intervaloService.listar(), []);
  const { dados: aluno, recarregar: recarregarAluno } = useApi(() => alunoService.detalhe(usuario.id), [usuario.id]);
  const intervalo = intervalos?.find(i => i.id === carrinho.intervaloId);

  // Mesmas regras que a API confere: janela aberta e teto de R$ 250 de fiado
  const aberto = janelaAberta(intervalo, carrinho.data);
  const teto = aluno && aluno.saldo - carrinho.total < TETO_FIADO;

  async function confirmar() {
    setEnviando(true);
    try {
      const pedido = await pedidoService.criar({
        data: carrinho.data,
        intervaloId: carrinho.intervaloId,
        itens: carrinho.itens.map(l => ({ itemId: l.item.id, quantidade: l.quantidade })),
      });
      carrinho.limpar();
      recarregarAluno();
      setFeedback({ sucesso: `Pedido feito para ${pedido.intervalo} de ${dataBR(pedido.data)}. Código de retirada: ${pedido.codigoRetirada}` });
    } catch (e) {
      setFeedback({ erro: e.message });
    } finally {
      setEnviando(false);
    }
  }

  return <>
    <div className="section-head"><h1>Revisão do pedido</h1>{intervalo && <p className="muted" style={{ margin: '8px 0 0' }}>{intervalo.nome} de {dataBR(carrinho.data)} · retirada às {hora(intervalo.horaInicio)}</p>}</div>
    <Feedback {...feedback}/>

    <div className="two-column" style={{ marginTop: 16 }}>
      <section className="surface pad">
        <h2>Itens</h2>
        {carrinho.itens.map(({ item, quantidade }) => <div className="line" key={item.id}>
          <div><strong>{item.nome}</strong><div className="small muted">{dinheiro(item.precoUnitario)} cada</div></div>
          <div className="row"><Quantidade nome={item.nome} valor={quantidade} maximo={item.estoque} aoMudar={q => carrinho.definirQuantidade(item, q)}/><strong style={{ minWidth: 80, textAlign: 'right' }}>{dinheiro(item.precoUnitario * quantidade)}</strong></div>
        </div>)}
        {!carrinho.itens.length && <div className="empty">Nenhum item selecionado. <Link className="text-button" to="/cardapio">Voltar ao cardápio</Link></div>}
      </section>

      <aside className="surface pad sticky-panel stack">
        <div className="between"><span>Total</span><strong className="metric">{dinheiro(carrinho.total)}</strong></div>
        <div className="small muted">Pagamento na sua conta · saldo após o pedido: {aluno ? dinheiro(aluno.saldo - carrinho.total) : '…'}</div>
        {teto && <div className="alert">Limite de R$ 250,00 atingido — somente à vista</div>}
        {intervalo && !aberto && <div className="alert">Pedidos para este intervalo já fecharam</div>}
        <button className="btn full" disabled={!carrinho.itens.length || !aberto || teto || enviando} onClick={confirmar}>{enviando ? 'Confirmando...' : 'Confirmar pedido'}</button>
      </aside>
    </div>
  </>;
}
