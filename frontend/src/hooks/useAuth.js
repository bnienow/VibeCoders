import { useContext } from 'react';
import { AuthContext } from '../contexts/AuthContext';

// Usuário logado e ações de sessão: const { usuario, entrar, cadastrar, sair } = useAuth();
export function useAuth() {
  return useContext(AuthContext);
}
