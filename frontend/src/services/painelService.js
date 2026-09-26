import { api } from './api';

// Rotas do PainelController (/api/painel) — só Admin
export const painelService = {
  intervaloAtual: () => api('/painel/intervalo-atual'),
  preparo: () => api('/painel/preparo'),
  buscar: (termo) => api(`/painel/buscar?termo=${encodeURIComponent(termo)}`),
};
