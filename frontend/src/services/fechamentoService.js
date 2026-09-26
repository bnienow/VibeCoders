import { api } from './api';

// Rotas do FechamentosController (/api/fechamentos)
export const fechamentoService = {
  doAdulto: (adultoId) => api(`/fechamentos/adulto/${adultoId}`),
  gerar: () => api('/fechamentos/gerar', { metodo: 'POST' }),
  pagar: (id) => api(`/fechamentos/${id}/pagar`, { metodo: 'POST' }),
};
