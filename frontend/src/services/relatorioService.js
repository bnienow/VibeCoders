import { api } from './api';

// Rotas do RelatoriosController (/api/relatorios) — datas "2026-09-01"
const periodo = (inicio, fim) => `inicio=${inicio}&fim=${fim}`;

export const relatorioService = {
  vendas: (inicio, fim) => api(`/relatorios/vendas?${periodo(inicio, fim)}`),
  itensMaisVendidos: (inicio, fim) => api(`/relatorios/itens-mais-vendidos?${periodo(inicio, fim)}`),

  // Endereço do Excel gerado pela API (baixa direto no navegador)
  urlExcel: (inicio, fim) => `/api/relatorios/vendas/excel?${periodo(inicio, fim)}`,
};
