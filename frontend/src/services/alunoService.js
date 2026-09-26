import { api } from './api';

// Rotas do AlunosController (/api/alunos)
export const alunoService = {
  // O responsável logado cadastra um filho: { nome, email, senha, turma, dataNascimento, restricoes: [] }
  cadastrar: (dados) => api('/alunos', { metodo: 'POST', corpo: dados }),
};
