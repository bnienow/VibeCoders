import { useState } from 'react';
import Layout from '../components/Layout';
import Feedback from '../components/Feedback';
import Quantidade from '../components/Quantidade';
import { useCantina } from '../hooks/useCantina';
import { intervaloAberto, intervalos } from '../services/catalogo';
import { money } from '../services/cantinaService';
export default function RevisaoPedido() {
  const { estado, usuario, itensCarrinho, totalCarrinho, service } = useCantina();
  const [forma,setForma] = useState('Conta'), [feedback,setFeedback] = useState({});
  const aluno = usuario?.papel === 'Aluno' ? usuario : estado.usuarios.find((u) => u.id === 'aluno-1');
  const teto = forma === 'Conta' && aluno && aluno.saldo-totalCarrinho < -250;
  function submit() { try { const p=service.confirmarPedido(forma); setFeedback({ sucesso:'Pedido '+p.id+' criado como Aberto. Código: '+p.codigoRetirada }); } catch(e) { setFeedback({ erro:e.message }); } }
  return <Layout perfil="Aluno" nome={aluno?.nome} saldo={aluno?.saldo}><div className="section-head"><span className="eyebrow">Conferência</span><h1>Revise seu pedido</h1><p className="lead">Intervalo {intervalos[estado.intervalo].nome.toLowerCase()} · {intervalos[estado.intervalo].inicio}. Ajuste quantidades antes de confirmar.</p></div><Feedback {...feedback}/>
    <div className="two-column" style={{marginTop:20}}><section className="surface pad"><h2>Itens selecionados</h2>{itensCarrinho.map((i) => <div className="line" key={i.id}><div><strong>{i.nome}</strong><div className="small muted">{money(i.preco)} por unidade</div></div><Quantidade nome={i.nome} valor={i.quantidade} maximo={i.estoque} aoMudar={(q) => service.definirQuantidade(i.id,q)}/><strong>{money(i.preco*i.quantidade)}</strong></div>)}{!itensCarrinho.length && <div className="empty">Nenhum item selecionado.</div>}</section>
      <aside className="surface pad sticky-panel"><h2>Resumo</h2><div className="between"><span>Total</span><strong className="metric">{money(totalCarrinho)}</strong></div><p className="small muted">Saldo projetado: {money((aluno?.saldo || 0)-totalCarrinho)}</p><label className="field">Forma de pagamento<select className="input" value={forma} onChange={(e) => setForma(e.target.value)}><option value="Conta">Lançar na conta</option><option value="AVista">À vista (demonstrativo)</option></select></label>{teto && <div className="alert" style={{marginTop:14}}>Limite de R$ 250,00 atingido — somente à vista</div>}<button className="btn full" style={{marginTop:18}} disabled={!itensCarrinho.length || !intervaloAberto(estado.intervalo) || teto} onClick={submit}>Confirmar pedido</button>{!intervaloAberto(estado.intervalo) && <p className="alert" style={{marginTop:12}}>Pedidos para este intervalo já fecharam</p>}</aside></div></Layout>;
}

