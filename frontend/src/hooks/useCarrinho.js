import { useContext } from 'react';
import { CarrinhoContext } from '../contexts/CarrinhoContext';

// const { data, intervaloId, itens, total, escolher, definirQuantidade, quantidade, limpar } = useCarrinho();
export function useCarrinho() {
  return useContext(CarrinhoContext);
}
