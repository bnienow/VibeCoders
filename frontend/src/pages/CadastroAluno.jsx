import { useState } from 'react';
import { alunoService } from '../services/alunoService';
import { dataISO, DOMINIO_ALUNO, limparEmail } from '../services/formatos';

const inicial = { nome: '', usuarioEmail: '', turma: '', nascimento: '', restricoes: '', senha: '', confirmar: '' };

// O responsável logado cadastra um filho. O e-mail é sempre do domínio da escola: digita-se só a parte antes do @.
export default function CadastroAluno() {
  const [campos, setCampos] = useState(inicial), [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState(''), [ocupado, setOcupado] = useState(false);
  const alterar = (campo, valor) => setCampos(anterior => ({ ...anterior, [campo]: valor }));

  async function enviar(evento) {
    evento.preventDefault();
    setErro(''); setSucesso('');
    if (campos.senha !== campos.confirmar) { setErro('As senhas não coincidem.'); return; }

    setOcupado(true);
    try {
      const email = `${campos.usuarioEmail}@${DOMINIO_ALUNO}`;
      await alunoService.cadastrar({
        nome: campos.nome.trim(), email, senha: campos.senha, turma: campos.turma.trim(),
        dataNascimento: campos.nascimento,
        restricoes: campos.restricoes.split(',').map(r => r.trim()).filter(Boolean), // "Lactose, Amendoim" → ["Lactose", "Amendoim"]
      });
      setCampos(inicial);
      setSucesso(`Aluno cadastrado. Ele já pode entrar com ${email}.`);
    } catch (falha) {
      setErro(falha.message);
    } finally {
      setOcupado(false);
    }
  }

  return <><div className="section-head"><h1>Cadastrar filho</h1></div>
    <form className="surface pad stack" style={{ maxWidth: 720 }} onSubmit={enviar}>
      <label className="field">Nome completo
        <input className="input" value={campos.nome} onChange={e => alterar('nome', e.target.value)} required />
      </label>
      <label className="field">E-mail institucional
        <div className="input-group">
          <input className="input" autoCapitalize="none" spellCheck={false} value={campos.usuarioEmail} onChange={e => alterar('usuarioEmail', limparEmail(e.target.value).split('@')[0])} required />
          <span className="addon">@{DOMINIO_ALUNO}</span>
        </div>
      </label>
      <div className="form-grid">
        <label className="field">Turma
          <input className="input" value={campos.turma} onChange={e => alterar('turma', e.target.value)} required />
        </label>
        <label className="field">Data de nascimento
          <input className="input" type="date" max={dataISO()} value={campos.nascimento} onChange={e => alterar('nascimento', e.target.value)} required />
        </label>
      </div>
      <label className="field">Restrições alimentares (separadas por vírgula)
        <input className="input" value={campos.restricoes} onChange={e => alterar('restricoes', e.target.value)} />
      </label>
      <div className="form-grid">
        <label className="field">Senha
          <input className="input" type="password" autoComplete="new-password" value={campos.senha} onChange={e => alterar('senha', e.target.value)} minLength={6} required />
        </label>
        <label className="field">Confirmar senha
          <input className="input" type="password" autoComplete="new-password" value={campos.confirmar} onChange={e => alterar('confirmar', e.target.value)} minLength={6} required />
        </label>
      </div>
      {erro && <div className="alert" role="alert">{erro}</div>}
      {sucesso && <div className="success" role="status">{sucesso}</div>}
      <div><button className="btn" type="submit" disabled={ocupado}>{ocupado ? 'Cadastrando...' : 'Cadastrar filho'}</button></div>
    </form></>;
}
