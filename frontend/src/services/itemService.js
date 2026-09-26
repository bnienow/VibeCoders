import { api } from './api';

// Rotas do ItensController (/api/itens e /api/cardapio)
export const itemService = {
  // Com intervaloId: só o que é oferecido naquele dia/intervalo. Sem: tudo ativo e com estoque (balcão).
  cardapio: (data, intervaloId) => api(`/itens/cardapio?${new URLSearchParams(
    Object.fromEntries(Object.entries({ data, intervaloId }).filter(([, v]) => v != null)))}`),

  listar: () => api('/itens'),

  // item: { nome, descricao, precoUnitario, estoque, categoria, alergenos: [] }
  criar: (item) => api('/itens', { metodo: 'POST', corpo: item }),
  editar: (id, item) => api(`/itens/${id}`, { metodo: 'PUT', corpo: item }),
  desativar: (id) => api(`/itens/${id}`, { metodo: 'DELETE' }),

  definirDisponibilidade: (data, intervaloId, itemId, disponivel) =>
    api(`/cardapio/${data}/${intervaloId}/${itemId}`, { metodo: 'PUT', corpo: { disponivel } }),
};
