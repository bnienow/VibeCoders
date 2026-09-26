import { useState } from 'react';
import Extrato from '../components/Extrato';
import { useApi } from '../hooks/useApi';
import { useAuth } from '../hooks/useAuth';
import { alunoService } from '../services/alunoService';
import { extratoService } from '../services/extratoService';
import { dataISO } from '../services/formatos';

export default function ExtratoFilho() {
  const { usuario } = useAuth();
  const { dados: filhos } = useApi(() => alunoService.filhos(usuario.id), [usuario.id]);
  const [selecionado, setSelecionado] = useState(null), [mes, setMes] = useState(dataISO().slice(0, 7));
  const filho = filhos?.find(f => f.id === selecionado) ?? filhos?.[0];
  const { dados: extrato } = useApi(() => filho ? extratoService.doAluno(filho.id, mes) : Promise.resolve(null), [filho?.id, mes]);

  return <>
    <div className="section-head"><h1>Extrato do filho</h1></div>
    <div className="surface pad toolbar no-print" style={{ marginBottom: 20 }}>
      <label className="field">Filho
        <select className="input" value={filho?.id ?? ''} onChange={e => setSelecionado(Number(e.target.value))}>
          {filhos?.map(f => <option value={f.id} key={f.id}>{f.nome}</option>)}
        </select>
      </label>
      <label className="field">Mês<input className="input" type="month" value={mes} onChange={e => setMes(e.target.value)}/></label>
      {filho && <a className="btn secondary" href={extratoService.urlPdf(filho.id, mes)} target="_blank" rel="noreferrer">Baixar PDF</a>}
    </div>
    <Extrato extrato={extrato}/>
    {filhos && !filhos.length && <div className="empty">Nenhum filho cadastrado.</div>}
  </>;
}
