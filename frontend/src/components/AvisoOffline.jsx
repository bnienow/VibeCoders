import { useEffect, useRef, useState } from 'react';
import { useConexao } from '../hooks/useConexao';
import { authService } from '../services/authService';
import { filaOffline } from '../services/filaOffline';
import { pedidoService } from '../services/pedidoService';

const INTERVALO_TENTATIVA_MS = 5000;

const TEXTO_SITUACAO = {
  Aceita: 'registrada',
  ConvertidaParaAVista: 'estourou o limite enquanto estava sem conexão: cobrar à vista',
  Rejeitada: 'não registrada',
};

// Faixa do modo offline (D6): avisa que a API caiu, mostra as vendas guardadas
// e, quando a conexão volta, envia a fila para a API reprocessar e mostra o resultado de cada venda.
export default function AvisoOffline() {
  const { online, pendentes } = useConexao();
  const [resultados, setResultados] = useState([]);
  const enviando = useRef(false);

  // Sem conexão: tenta falar com a API de tempos em tempos (qualquer resposta a marca como online de novo)
  useEffect(() => {
    if (online) return;
    const id = setInterval(() => authService.usuarioLogado().catch(() => {}), INTERVALO_TENTATIVA_MS);
    return () => clearInterval(id);
  }, [online]);

  // Conexão de volta com vendas na fila: envia todas para /api/pedidos/sincronizar
  useEffect(() => {
    if (!online || !pendentes.length || enviando.current) return;
    enviando.current = true;
    const enviadas = pendentes;

    pedidoService.sincronizar(enviadas.map((p) => p.venda))
      .then((respostas) => {
        filaOffline.removerPrimeiras(enviadas.length);
        setResultados(respostas.map((r) => ({ ...r, rotulo: enviadas[r.indice].rotulo })));
      })
      .catch(() => {}) // caiu de novo no meio do envio: a fila continua guardada
      .finally(() => { enviando.current = false; });
  }, [online, pendentes]);

  if (online && !pendentes.length && !resultados.length) return null;

  return <div className={`faixa-offline ${online ? 'ok' : ''}`}><div className="container stack" style={{ gap: 6 }}>
    {!online && <strong>Sem conexão com o servidor. O balcão continua vendendo; as vendas ficam guardadas.</strong>}
    {pendentes.length > 0 && <span>{pendentes.length} venda(s) guardada(s){online ? ': enviando...' : ', serão enviadas quando a conexão voltar.'}</span>}
    {resultados.length > 0 && <>
      <strong>Vendas guardadas enviadas:</strong>
      {resultados.map((r) => <span key={r.indice} className={r.situacao === 'Aceita' ? '' : 'atencao'}>
        {r.rotulo}: {TEXTO_SITUACAO[r.situacao]}{r.motivo ? ` (${r.motivo})` : ''}
      </span>)}
      <div><button className="text-button" onClick={() => setResultados([])}>Fechar</button></div>
    </>}
  </div></div>;
}
