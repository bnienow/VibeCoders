import { api } from './api';

// Rotas do PedidosController (/api/pedidos)
export const pedidoService = {
  // Pedido antecipado: { data, intervaloId, itens: [{ itemId, quantidade }] }
  criar: (pedido) => api('/pedidos', { metodo: 'POST', corpo: pedido }),

  // Venda do Admin no balcão: { usuarioId, formaPagamento: 'Conta' | 'AVista', itens }
  venderNoBalcao: (venda) => api('/pedidos/balcao', { metodo: 'POST', corpo: venda }),
  sincronizar: (vendas) => api('/pedidos/sincronizar', { metodo: 'POST', corpo: vendas }),

  meus: () => api('/pedidos/meus'),
  detalhe: (id) => api(`/pedidos/${id}`),
  alterar: (id, itens) => api(`/pedidos/${id}`, { metodo: 'PUT', corpo: { itens } }),
  cancelar: (id) => api(`/pedidos/${id}`, { metodo: 'DELETE' }),
  entregar: (id) => api(`/pedidos/${id}/entregar`, { metodo: 'POST' }),
};
