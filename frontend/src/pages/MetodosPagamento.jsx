import { useState } from 'react';
import Feedback from '../components/Feedback';
import { useApi } from '../hooks/useApi';
import { pagamentoService } from '../services/pagamentoService';

const vazio = { tipo: 'Pix', apelido: '', ultimosDigitos: '', padrao: false };

export default function MetodosPagamento() {
  const { dados: metodos, recarregar } = useApi(() => pagamentoService.metodos(), []);
  const [form, setForm] = useState(vazio), [feedback, setFeedback] = useState({});

  async function salvar(e) {
    e.preventDefault();
    try {
      await pagamentoService.cadastrarMetodo({ ...form, ultimosDigitos: form.tipo === 'Cartao' ? form.ultimosDigitos : '' });
      setForm(vazio);
      setFeedback({ sucesso: 'Método adicionado.' });
      recarregar();
    } catch (err) {
      setFeedback({ erro: err.message });
    }
  }

  return <>
    <div className="section-head"><h1>Métodos de pagamento</h1></div>
    <Feedback {...feedback}/>

    <div className="two-column" style={{ marginTop: 16 }}>
      <section className="surface pad">
        <h2>Cadastrados</h2>
        {metodos?.map(m => <div className="line" key={m.id}>
          <div><strong>{m.apelido}</strong><div className="small muted">{m.tipo === 'Pix' ? 'Pix' : `Cartão final ${m.ultimosDigitos}`}</div></div>
          {m.padrao && <span className="badge">Padrão</span>}
        </div>)}
        {metodos && !metodos.length && <div className="empty">Nenhum método cadastrado.</div>}
      </section>

      <form className="surface pad card-form" onSubmit={salvar}>
        <h2>Novo método</h2>
        <label className="field">Tipo
          <select className="input" value={form.tipo} onChange={e => setForm({ ...form, tipo: e.target.value })}>
            <option value="Pix">Pix</option>
            <option value="Cartao">Cartão</option>
          </select>
        </label>
        <label className="field">Apelido<input className="input" required value={form.apelido} onChange={e => setForm({ ...form, apelido: e.target.value })}/></label>
        {form.tipo === 'Cartao' && <label className="field">Últimos 4 dígitos
          <input className="input" inputMode="numeric" maxLength={4} pattern="\d{4}" title="4 dígitos" required value={form.ultimosDigitos} onChange={e => setForm({ ...form, ultimosDigitos: e.target.value.replace(/\D/g, '') })}/>
        </label>}
        <label className="check"><input type="checkbox" checked={form.padrao} onChange={e => setForm({ ...form, padrao: e.target.checked })}/> Usar como padrão</label>
        <div className="acoes"><button className="btn" type="submit">Salvar método</button></div>
      </form>
    </div>
  </>;
}
