import { api } from './api';

// Rotas do AlunosController (/api/alunos e /api/adultos/{id}/filhos)
export const alunoService = {
  // Busca por nome ou e-mail (balcão do Admin)
  buscar: (busca) => api(`/alunos?busca=${encodeURIComponent(busca ?? '')}`),

  // Saldo, limite, restrições e gasto do mês
  detalhe: (id) => api(`/alunos/${id}`),

  // O responsável logado cadastra um filho: { nome, email, senha, turma, dataNascimento, restricoes: [] }
  cadastrar: (dados) => api('/alunos', { metodo: 'POST', corpo: dados }),

  filhos: (adultoId) => api(`/adultos/${adultoId}/filhos`),

  // limiteDiario null = sem limite
  definirLimite: (id, limiteDiario) => api(`/alunos/${id}/limite`, { metodo: 'PUT', corpo: { limiteDiario } }),
  definirRestricoes: (id, restricoes) => api(`/alunos/${id}/restricoes`, { metodo: 'PUT', corpo: { restricoes } }),
  adicionarCredito: (id, valor, metodoPagamentoId) =>
    api(`/alunos/${id}/credito`, { metodo: 'POST', corpo: { valor, metodoPagamentoId } }),
};
