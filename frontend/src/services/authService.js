import { api } from './api';

// Rotas do AuthController (/api/auth)
export const authService = {
  entrar: (email, senha) => api('/auth/login', { metodo: 'POST', corpo: { email, senha } }),

  // Cadastra um responsável: { nome, email, senha, cpf, telefone }
  cadastrar: (dados) => api('/auth/registro', { metodo: 'POST', corpo: dados }),

  sair: () => api('/auth/logout', { metodo: 'POST' }),

  // Quem está logado agora (lido do cookie). Sem sessão, a API responde 401 e isto lança erro.
  usuarioLogado: () => api('/auth/me'),
};
