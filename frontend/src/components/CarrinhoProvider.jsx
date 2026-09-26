import { useState } from 'react';
import { CarrinhoContext } from '../contexts/CarrinhoContext';
import { dataISO } from '../services/formatos';

// linhas: { [itemId]: { item, quantidade } } — guarda o item junto para a Revisão mostrar nome e preço
export default function CarrinhoProvider({ children }) {
  const [data, setData] = useState(dataISO());
  const [intervaloId, setIntervaloId] = useState(null);
  const [linhas, setLinhas] = useState({});

  function definirQuantidade(item, quantidade) {
    setLinhas((atuais) => {
      const proximas = { ...atuais };
      if (quantidade > 0) proximas[item.id] = { item, quantidade };
      else delete proximas[item.id];
      return proximas;
    });
  }

  // Trocar dia ou intervalo esvazia o carrinho: o cardápio pode ser outro.
  // A primeira escolha de intervalo (antes era nenhum) não esvazia: os itens foram escolhidos nele.
  function escolher(novaData, novoIntervaloId) {
    const mudou = novaData !== data || (intervaloId != null && novoIntervaloId !== intervaloId);
    setData(novaData);
    setIntervaloId(novoIntervaloId);
    if (mudou) setLinhas({});
  }

  const itens = Object.values(linhas);
  const total = itens.reduce((soma, l) => soma + l.item.precoUnitario * l.quantidade, 0);

  return <CarrinhoContext.Provider value={{
    data, intervaloId, itens, total, escolher, definirQuantidade,
    quantidade: (itemId) => linhas[itemId]?.quantidade ?? 0,
    limpar: () => setLinhas({}),
  }}>{children}</CarrinhoContext.Provider>;
}
