import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { limparEmail, mascaraCpf, mascaraTelefone, PADRAO_CPF, PADRAO_TELEFONE } from '../services/formatos';
import { PAGINA_INICIAL } from '../rotas';

const inicial = { nome: '', cpf: '', telefone: '', email: '', senha: '', confirmar: '' };

// Cadastro de responsável. O aluno é cadastrado depois, pelo responsável logado.
export default function CadastroAdulto() {
  const navigate = useNavigate();
  const { cadastrar } = useAuth();
  const [campos, setCampos] = useState(inicial), [erro, setErro] = useState('');
  const [ocupado, setOcupado] = useState(false), [mostrar, setMostrar] = useState(false);
  const alterar = (campo, valor) => setCampos(anterior => ({ ...anterior, [campo]: valor }));

  async function enviar(evento) {
    evento.preventDefault();
    setErro('');
    if (campos.senha !== campos.confirmar) { setErro('As senhas não coincidem.'); return; }

    setOcupado(true);
    try {
      await cadastrar({ nome: campos.nome.trim(), email: campos.email, senha: campos.senha, cpf: campos.cpf, telefone: campos.telefone });
      navigate(PAGINA_INICIAL.Adulto);
    } catch (falha) {
      setErro(falha.message);
    } finally {
      setOcupado(false);
    }
  }

  return <main className="auth"><section className="surface auth-card">
    <h1>Cadastro de responsável</h1>
    <form onSubmit={enviar}>
      <label className="field">Nome completo
        <input className="input" autoComplete="name" value={campos.nome} onChange={e => alterar('nome', e.target.value)} required />
      </label>
      <div className="form-grid">
        <label className="field">CPF
          <input className="input" inputMode="numeric" placeholder="000.000.000-00" maxLength={14} pattern={PADRAO_CPF} title="CPF no formato 000.000.000-00" value={campos.cpf} onChange={e => alterar('cpf', mascaraCpf(e.target.value))} required />
        </label>
        <label className="field">Telefone
          <input className="input" type="tel" inputMode="numeric" autoComplete="tel-national" placeholder="(00) 00000-0000" maxLength={15} pattern={PADRAO_TELEFONE} title="Telefone no formato (00) 00000-0000" value={campos.telefone} onChange={e => alterar('telefone', mascaraTelefone(e.target.value))} required />
        </label>
      </div>
      <label className="field">E-mail
        <input className="input" type="email" autoComplete="email" autoCapitalize="none" spellCheck={false} value={campos.email} onChange={e => alterar('email', limparEmail(e.target.value))} required />
      </label>
      <div className="form-grid">
        <label className="field">Senha
          <input className="input" type={mostrar ? 'text' : 'password'} autoComplete="new-password" value={campos.senha} onChange={e => alterar('senha', e.target.value)} minLength={6} required />
        </label>
        <label className="field">Confirmar senha
          <input className="input" type={mostrar ? 'text' : 'password'} autoComplete="new-password" value={campos.confirmar} onChange={e => alterar('confirmar', e.target.value)} minLength={6} required />
        </label>
      </div>
      <label className="check"><input type="checkbox" checked={mostrar} onChange={e => setMostrar(e.target.checked)} /> Mostrar senhas</label>
      {erro && <div className="alert" role="alert">{erro}</div>}
      <button className="btn full" type="submit" disabled={ocupado}>{ocupado ? 'Criando...' : 'Criar conta'}</button>
    </form>
    <p className="auth-bottom">Já tem uma conta? <Link className="text-button" to="/login">Entrar</Link></p>
  </section></main>;
}
