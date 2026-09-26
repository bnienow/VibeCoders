import Quantidade from './Quantidade';
import { dinheiro } from '../services/formatos';

// Cartão de item do cardápio. restricoes: alérgenos que o aluno não pode comer.
export default function ProdutoCard({ item, quantidade = 0, aoMudar, restricoes = [] }) {
  const conflito = item.alergenos.find((a) => restricoes.includes(a));

  return <article className={`surface product ${quantidade ? 'chosen' : ''}`}>
    <h3>{item.nome}</h3>
    <p>{item.descricao}</p>
    {item.alergenos.length > 0 && <p className="small">Alérgenos: {item.alergenos.join(', ')}</p>}
    {conflito && <span><span className="badge red">Contém {conflito.toLowerCase()}</span></span>}
    <div className="bottom">
      <span className="price">{dinheiro(item.precoUnitario)}</span>
      <Quantidade nome={item.nome} valor={quantidade} maximo={item.estoque} aoMudar={aoMudar} />
    </div>
  </article>;
}
