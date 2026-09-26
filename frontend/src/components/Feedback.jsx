export default function Feedback({ erro, sucesso }) {
  return <>{erro && <div className="alert" role="alert">{erro}</div>}{sucesso && <div className="success" role="status">{sucesso}</div>}</>;
}