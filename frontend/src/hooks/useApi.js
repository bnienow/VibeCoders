import { useEffect, useState } from 'react';

// Carrega dados da API ao abrir a tela e quando as dependências mudam.
// const { dados, erro, carregando, recarregar } = useApi(() => pedidoService.meus(), []);
export function useApi(buscar, dependencias) {
  const [estado, setEstado] = useState({ dados: null, erro: '', carregando: true });
  const [versao, setVersao] = useState(0);

  useEffect(() => {
    let ativo = true; // ignora a resposta se a tela já mudou antes dela chegar
    buscar()
      .then((dados) => ativo && setEstado({ dados, erro: '', carregando: false }))
      .catch((falha) => ativo && setEstado((anterior) => ({ ...anterior, erro: falha.message, carregando: false })));
    return () => { ativo = false; };
    // As dependências vêm de quem chama (como no useEffect); "versao" força recarregar
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...dependencias, versao]);

  return { ...estado, recarregar: () => setVersao((v) => v + 1) };
}
