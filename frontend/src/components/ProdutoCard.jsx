  import Quantidade from './Quantidade';
  import { money } from '../services/cantinaService';
  export default function ProdutoCard({ item, quantidade = 0, aoMudar, restricoes = [] }) {
    const conflito = item.alergenos?.find((a) => restricoes.some((r) => r.toLowerCase() === a.toLowerCase()));
    return <article className="surface product"><div className="image-slot" role="img" aria-label="Espaço reservado para imagem do item"/><h3>{item.nome}</h3><p>{item.descricao}</p><div className="row wrap">{conflito && <span className="badge red">Contém {conflito.toLowerCase()}</span>}{!item.estoque && <span className="badge amber">Esgotado</span>}</div><div className="bottom"><span className="price">{money(item.preco)}</span><Quantidade nome={item.nome} valor={quantidade} maximo={item.estoque} aoMudar={aoMudar}/></div></article>;
  }