import { api } from './api';

// Rotas do PagamentosController (/api/pagamentos) — pagamento simulado (D1)
export const pagamentoService = {
  metodos: () => api('/pagamentos/metodos'),

  // metodo: { tipo: 'Pix' | 'Cartao', apelido, ultimosDigitos, padrao }
  cadastrarMetodo: (metodo) => api('/pagamentos/metodos', { metodo: 'POST', corpo: metodo }),

  // Crédito na conta do responsável (usuarioId = ele mesmo) ou de um filho
  simular: (usuarioId, valor, metodoPagamentoId) =>
    api('/pagamentos/simular', { metodo: 'POST', corpo: { usuarioId, valor, metodoPagamentoId } }),
};
