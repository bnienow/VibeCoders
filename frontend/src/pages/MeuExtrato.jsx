import { useState } from 'react';
import { useApi } from '../hooks/useApi';
import { useAuth } from '../hooks/useAuth';
import Extrato from '../components/Extrato';
import { extratoService } from '../services/extratoService';
import { dataISO } from '../services/formatos';

export default function MeuExtrato() {
  const { usuario } = useAuth();
  const [mes, setMes] = useState(dataISO().slice(0, 7));
  const { dados: extrato } = useApi(() => extratoService.doAluno(usuario.id, mes), [usuario.id, mes]);

  return <>
    <div className="section-head"><h1>Meu extrato</h1></div>
    <div className="surface pad toolbar no-print" style={{ marginBottom: 20 }}>
      <label className="field">Mês<input className="input" type="month" value={mes} onChange={e => setMes(e.target.value)}/></label>
      <a className="btn secondary" href={extratoService.urlPdf(usuario.id, mes)} target="_blank" rel="noreferrer">Baixar PDF</a>
    </div>
    <Extrato extrato={extrato}/>
  </>;
}
