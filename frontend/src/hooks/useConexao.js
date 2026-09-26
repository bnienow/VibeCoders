import { useSyncExternalStore } from 'react';
import { conexao } from '../services/conexao';
import { filaOffline } from '../services/filaOffline';

// const { online, pendentes } = useConexao(); — a tela redesenha quando a conexão cai/volta ou a fila muda
export function useConexao() {
  const online = useSyncExternalStore(conexao.assinar, conexao.online);
  const pendentes = useSyncExternalStore(filaOffline.assinar, filaOffline.listar);
  return { online, pendentes };
}
