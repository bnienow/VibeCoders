import { useState } from 'react';
import CampoDinheiro from '../components/CampoDinheiro';
import Feedback from '../components/Feedback';
import { useApi } from '../hooks/useApi';
import { CATEGORIAS, dataISO } from '../services/formatos';
import { intervaloService } from '../services/intervaloService';
import { itemService } from '../services/itemService';

const vazio = { nome: '', descricao: '', categoria: 'Salgado', precoUnitario: null, estoque: '', alergenos: '' };

// "Lactose, Amendoim" → ["Lactose", "Amendoim"]
const paraLista = (texto) => texto.split(',').map(a => a.trim()).filter(Boolean);

export default function GestaoItens() {
  const hoje = dataISO();
  const { dados: itens, recarregar } = useApi(() => itemService.listar(), []);
  const { dados: intervalos } = useApi(() => intervaloService.listar(), []);

  // O que está no cardápio de hoje em cada intervalo: { [intervaloId]: Set(itemIds) }
  const { dados: ofertas, recarregar: recarregarOfertas } = useApi(async () => {
    if (!intervalos) return {};
    const cardapios = await Promise.all(intervalos.map(i => itemService.cardapio(hoje, i.id)));
    return Object.fromEntries(intervalos.map((i, n) => [i.id, new Set(cardapios[n].map(item => item.id))]));
  }, [intervalos]);

  const [form, setForm] = useState(vazio), [feedback, setFeedback] = useState({}), [busca, setBusca] = useState(''), [edicao, setEdicao] = useState({});
  const visiveis = (itens ?? []).filter(i => i.nome.toLowerCase().includes(busca.toLowerCase()));

  // Executa a ação na API, mostra o resultado e recarrega itens e ofertas
  async function executar(acao, mensagem) {
    try {
      await acao();
      setFeedback({ sucesso: mensagem });
      recarregar();
      recarregarOfertas();
      return true;
    } catch (e) {
      setFeedback({ erro: e.message });
      return false;
    }
  }

  async function criar(e) {
    e.preventDefault();
    const ok = await executar(() => itemService.criar({ ...form, estoque: Number(form.estoque), alergenos: paraLista(form.alergenos) }), 'Item criado.');
    if (ok) setForm(vazio);
  }

  // Salva preço e estoque editados na linha (o PUT recebe o item completo)
  function salvarLinha(i) {
    const editado = edicao[i.id] ?? {};
    executar(() => itemService.editar(i.id, {
      nome: i.nome, descricao: i.descricao, categoria: i.categoria, alergenos: i.alergenos,
      precoUnitario: precoNaTela(i),
      estoque: Number(editado.estoque ?? i.estoque),
    }), `${i.nome} atualizado.`);
  }

  const editar = (id, campo, valor) => setEdicao({ ...edicao, [id]: { ...edicao[id], [campo]: valor } });

  // Preço mostrado na linha: o editado (mesmo que apagado, null) ou, se não mexeu, o do banco
  const precoNaTela = (i) => edicao[i.id] && 'precoUnitario' in edicao[i.id] ? edicao[i.id].precoUnitario : i.precoUnitario;

  return <>
    <div className="section-head"><h1>Itens</h1></div>
    <Feedback {...feedback}/>

    <form className="surface pad stack" style={{ margin: '16px 0 20px' }} onSubmit={criar}>
      <h2 style={{ margin: 0 }}>Novo item</h2>
      <div className="grid-3">
        <label className="field">Nome<input className="input" required value={form.nome} onChange={e => setForm({ ...form, nome: e.target.value })}/></label>
        <label className="field">Categoria
          <select className="input" value={form.categoria} onChange={e => setForm({ ...form, categoria: e.target.value })}>
            {Object.entries(CATEGORIAS).map(([valor, titulo]) => <option key={valor} value={valor}>{titulo}</option>)}
          </select>
        </label>
        <label className="field">Alérgenos (separados por vírgula)<input className="input" value={form.alergenos} onChange={e => setForm({ ...form, alergenos: e.target.value })}/></label>
      </div>
      <div className="toolbar">
        <label className="field" style={{ flex: '3 1 300px' }}>Descrição<input className="input" required value={form.descricao} onChange={e => setForm({ ...form, descricao: e.target.value })}/></label>
        <label className="field">Preço<CampoDinheiro valor={form.precoUnitario} aoMudar={v => setForm({ ...form, precoUnitario: v })} required/></label>
        <label className="field">Estoque<input className="input" type="number" min="0" step="1" required value={form.estoque} onChange={e => setForm({ ...form, estoque: e.target.value })}/></label>
        <button className="btn" type="submit">Criar item</button>
      </div>
    </form>

    <section className="surface pad">
      <div className="toolbar" style={{ marginBottom: 12 }}>
        <label className="field">Buscar item<input className="input" type="search" value={busca} onChange={e => setBusca(e.target.value)}/></label>
      </div>
      <div className="table-scroll"><table className="data-table">
        <thead><tr><th>Item</th><th>Preço</th><th>Estoque</th><th>Alérgenos</th><th>Cardápio de hoje</th><th>Ativo</th><th></th></tr></thead>
        <tbody>{visiveis.map(i => <tr key={i.id}>
          <td><strong>{i.nome}</strong><div className="small muted">{CATEGORIAS[i.categoria]} · {i.descricao}</div></td>
          <td><CampoDinheiro aria-label={`Preço de ${i.nome}`} style={{ width: 116 }} valor={precoNaTela(i)} aoMudar={v => editar(i.id, 'precoUnitario', v)}/></td>
          <td><input aria-label={`Estoque de ${i.nome}`} className="input" type="number" min="0" step="1" style={{ width: 84 }} value={edicao[i.id]?.estoque ?? i.estoque} onChange={e => editar(i.id, 'estoque', e.target.value)}/></td>
          <td className="small">{i.alergenos.join(', ') || '—'}</td>
          <td><div className="row">{intervalos?.map(intervalo => <label key={intervalo.id} className="check">
            <input type="checkbox" checked={Boolean(ofertas?.[intervalo.id]?.has(i.id))} onChange={e => executar(() => itemService.definirDisponibilidade(hoje, intervalo.id, i.id, e.target.checked), 'Cardápio de hoje atualizado.')}/>{intervalo.nome}
          </label>)}</div></td>
          <td><span className={`badge ${i.ativo ? '' : 'amber'}`}>{i.ativo ? 'Sim' : 'Não'}</span></td>
          <td><div className="row" style={{ justifyContent: 'flex-end' }}>
            <button className="btn soft small" onClick={() => salvarLinha(i)}>Salvar</button>
            {i.ativo && <button className="btn danger small" onClick={() => executar(() => itemService.desativar(i.id), `${i.nome} desativado.`)}>Desativar</button>}
          </div></td>
        </tr>)}</tbody>
      </table></div>
    </section>
  </>;
}
