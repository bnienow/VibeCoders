// Vendas de balcão feitas sem conexão, guardadas no navegador até a API voltar.
// Cada item: { venda: { usuarioId, formaPagamento, itens }, rotulo: "Pedro Kunz · R$ 7,00" }
const CHAVE = 'cantina.vendasPendentes';
const ouvintes = new Set();

function ler() {
  try {
    return JSON.parse(localStorage.getItem(CHAVE)) ?? [];
  } catch {
    return [];
  }
}

function gravar(lista) {
  localStorage.setItem(CHAVE, JSON.stringify(lista));
  ouvintes.forEach((ouvinte) => ouvinte());
}

// Cache da leitura: o useSyncExternalStore exige devolver o mesmo objeto enquanto nada muda
let atual = ler();

export const filaOffline = {
  listar: () => atual,

  adicionar(venda, rotulo) {
    atual = [...atual, { venda, rotulo }];
    gravar(atual);
  },

  // Tira da fila as primeiras "quantidade" vendas (as que foram enviadas); as feitas durante o envio ficam
  removerPrimeiras(quantidade) {
    atual = atual.slice(quantidade);
    gravar(atual);
  },

  assinar(ouvinte) {
    ouvintes.add(ouvinte);
    return () => ouvintes.delete(ouvinte);
  },
};
