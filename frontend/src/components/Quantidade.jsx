export default function Quantidade({ nome, valor, maximo, aoAlterar }) {
  return <span className="quantity" role="group" aria-label={`Quantidade de ${nome}`}>
    <button type="button" aria-label={`Diminuir ${nome}`} disabled={valor === 0} onClick={() => aoAlterar(valor - 1)}>−</button>
    <output aria-live="polite">{valor}</output>
    <button type="button" aria-label={`Aumentar ${nome}`} disabled={valor >= maximo} onClick={() => aoAlterar(valor + 1)}>+</button>
  </span>;
}