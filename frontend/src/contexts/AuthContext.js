import { createContext } from 'react';

// Guarda o usuário logado para o app inteiro. Preenchido pelo AuthProvider, lido com useAuth().
export const AuthContext = createContext(null);
