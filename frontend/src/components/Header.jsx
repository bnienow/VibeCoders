import { money } from '../services/cantinaService';
export default function Header({ perfil = 'Aluno', nome, saldo }) {
  return <header className="page-header"><div className="container header-inner">
    <div className="row"><div className="logo">Cantina<span> do Pátio</span></div><span className="role">{perfil}</span></div>
    <div className="header-actions"><strong className="small">{nome || 'Prévia demonstrativa'}</strong>{perfil === 'Aluno' && saldo != null && <span className={'badge ' + (saldo < 0 ? 'red' : '')}>Saldo {money(saldo)}</span>}</div>
  </div></header>;
}