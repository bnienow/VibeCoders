import { useState } from 'react';
import CampoDinheiro from '../components/CampoDinheiro';
import Feedback from '../components/Feedback';
import { useApi } from '../hooks/useApi';
import { useAuth } from '../hooks/useAuth';
import { alunoService } from '../services/alunoService';
import { dinheiro } from '../services/formatos';
import { pagamentoService } from '../services/pagamentoService';

export default function GestaoFilhos() {
  const { usuario } = useAuth();
  const { dados: filhos, recarregar } = useApi(() => alunoService.filhos(usuario.id), [usuario.id]);
  const { dados: metodos } = useApi(() => pagamentoService.metodos(), []);
  const [selecionado, setSelecionado] = useState(null), [valor, setValor] = useState(null), [limite, setLimite] = useState(null);
  const [restricoes, setRestricoes] = useState(''), [metodo, setMetodo] = useState(''), [feedback, setFeedback] = useState({});
  const filho = filhos?.find(f => f.id === selecionado) ?? filhos?.[0];

  // Executa a ação na API, mostra o resultado e recarrega os filhos (saldo, limite, restrições)
  async function executar(acao, mensagem) {
    try {
      await acao();
      setFeedback({ sucesso: mensagem });
      recarregar();
    } catch (e) {
      setFeedback({ erro: e.message });
    }
  }

  function adicionarCredito(evento) {
    evento.preventDefault();
    executar(() => alunoService.adicionarCredito(filho.id, valor, Number(metodo)), `Crédito de ${dinheiro(valor)} adicionado.`);
    setValor(null);
  }

  return <>
    <div className="section-head"><h1>Gestão dos filhos</h1></div>
    <Feedback {...feedback}/>
    <div className="pill-tabs" style={{ margin: '16px 0 20px' }}>{filhos?.map(f => <button key={f.id} aria-pressed={filho?.id === f.id} onClick={() => { setSelecionado(f.id); setFeedback({}); }}>{f.nome}</button>)}</div>

    {filho ? <>
      <div className="surface pad between wrap" style={{ marginBottom: 20 }}>
        <div><h2 style={{ margin: 0 }}>{filho.nome}</h2><span className="small muted">{filho.turma} · {filho.email}</span></div>
        <div className="right"><span className="small muted">Saldo</span><div className="metric">{dinheiro(filho.saldo)}</div></div>
      </div>

      <div className="grid-3">
        <form className="surface pad card-form" onSubmit={adicionarCredito}>
          <h2>Adicionar crédito</h2>
          <label className="field">Valor<CampoDinheiro valor={valor} aoMudar={setValor} required/></label>
          <label className="field">Método de pagamento
            <select className="input" value={metodo} onChange={e => setMetodo(e.target.value)} required>
              <option value="">Selecione</option>
              {metodos?.map(m => <option value={m.id} key={m.id}>{m.apelido}</option>)}
            </select>
          </label>
          <div className="acoes"><button className="btn" type="submit">Adicionar crédito</button></div>
        </form>

        <section className="surface pad card-form">
          <h2>Limite diário</h2>
          <span className="small muted">Atual: {filho.limiteDiario == null ? 'sem limite' : dinheiro(filho.limiteDiario)}</span>
          <label className="field">Novo limite<CampoDinheiro valor={limite} aoMudar={setLimite}/></label>
          <div className="acoes">
            <button className="btn" disabled={!limite} onClick={() => executar(() => alunoService.definirLimite(filho.id, limite), 'Limite diário atualizado.')}>Salvar limite</button>
            <button className="btn secondary" onClick={() => executar(() => alunoService.definirLimite(filho.id, null), 'Limite removido.')}>Deixar sem limite</button>
          </div>
        </section>

        <section className="surface pad card-form">
          <h2>Restrições</h2>
          <span className="small muted">Atuais: {filho.restricoes.join(', ') || 'nenhuma'}</span>
          <label className="field">Alérgenos (separados por vírgula)<input className="input" value={restricoes} onChange={e => setRestricoes(e.target.value)}/></label>
          <div className="acoes"><button className="btn" onClick={() => executar(() => alunoService.definirRestricoes(filho.id, restricoes.split(',').map(r => r.trim()).filter(Boolean)), 'Restrições atualizadas.')}>Salvar restrições</button></div>
        </section>
      </div>
    </> : filhos && <div className="empty">Nenhum filho cadastrado.</div>}
  </>;
}
