import { useEffect, useState } from 'react';
import { AuthContext } from '../contexts/AuthContext';
import { authService } from '../services/authService';

// Mantém quem está logado ({ id, nome, email, permissao }) e expõe entrar, cadastrar e sair.
export default function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null);
  const [carregando, setCarregando] = useState(true);

  // Ao abrir o app, pergunta à API se o cookie ainda tem uma sessão válida
  useEffect(() => {
    authService.usuarioLogado()
      .then(setUsuario)
      .catch(() => setUsuario(null))
      .finally(() => setCarregando(false));
  }, []);

  async function entrar(email, senha) {
    const logado = await authService.entrar(email, senha);
    setUsuario(logado);
    return logado;
  }

  // O registro não cria sessão: depois de cadastrar, já entra com o mesmo e-mail e senha
  async function cadastrar(dados) {
    await authService.cadastrar(dados);
    return entrar(dados.email, dados.senha);
  }

  async function sair() {
    await authService.sair();
    setUsuario(null);
  }

  return <AuthContext.Provider value={{ usuario, carregando, entrar, cadastrar, sair }}>{children}</AuthContext.Provider>;
}
