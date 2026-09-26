import { createContext } from 'react';

// Carrinho do pedido antecipado (dia, intervalo e itens), compartilhado entre Cardápio, Catálogo e Revisão
export const CarrinhoContext = createContext(null);
