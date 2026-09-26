import { api } from './api';

// Rotas do ExtratosController (/api/extratos). mes no formato "2026-08".
export const extratoService = {
  doAluno: (alunoId, mes) => api(`/extratos/aluno/${alunoId}?mes=${mes}`),

  // Endereço do PDF gerado pela API (abre/baixa direto no navegador, com o cookie de sessão)
  urlPdf: (alunoId, mes) => `/api/extratos/aluno/${alunoId}/pdf?mes=${mes}`,
};
