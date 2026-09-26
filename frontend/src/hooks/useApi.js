import { useEffect, useState } from 'react';
import { SemConexaoError } from '../services/conexao';

// Carrega dados da API ao abrir a tela e quando as dependências mudam.
// const { dados, erro, carregando, recarregar } = useApi(() => pedidoService.meus(), []);
//
// chaveCache (opcional): guarda cada resposta no navegador e, se a API cair,
// devolve a última cópia guardada (usado pelo balcão para funcionar sem conexão).
export function useApi(buscar, dependencias, chaveCache) {
  const [estado, setEstado] = useState({ dados: null, erro: '', carregando: true });
  const [versao, setVersao] = useState(0);

  useEffect(() => {
    let ativo = true; // ignora a resposta se a tela já mudou antes dela chegar

    buscar()
      .then((dados) => {
        if (chaveCache) guardarCopia(chaveCache, dados);
        if (ativo) setEstado({ dados, erro: '', carregando: false });
      })
      .catch((falha) => {
        if (!ativo) return;
        const copia = falha instanceof SemConexaoError && chaveCache ? lerCopia(chaveCache) : null;
        setEstado((anterior) => copia
          ? { dados: copia, erro: '', carregando: false }
          : { ...anterior, erro: falha.message, carregando: false });
      });

    return () => { ativo = false; };
    // As dependências vêm de quem chama (como no useEffect); "versao" força recarregar
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...dependencias, versao]);

  return { ...estado, recarregar: () => setVersao((v) => v + 1) };
}

function guardarCopia(chave, dados) {
  try { localStorage.setItem(`cantina.copia.${chave}`, JSON.stringify(dados)); } catch { /* sem espaço: segue sem cópia */ }
}

function lerCopia(chave) {
  try { return JSON.parse(localStorage.getItem(`cantina.copia.${chave}`)); } catch { return null; }
}
