// Estado da conexão com a API. Não é a internet: na demo tudo roda local, então o que "cai" é a API.
// Quem muda o estado é o api.js: falha de rede (ou 502 do proxy do Vite) → offline; qualquer resposta da API → online.
let online = true;
const ouvintes = new Set();

export const conexao = {
  online: () => online,

  marcar(valor) {
    if (valor === online) return;
    online = valor;
    ouvintes.forEach((ouvinte) => ouvinte());
  },

  // Avisa quem estiver ouvindo (useConexao) quando o estado muda
  assinar(ouvinte) {
    ouvintes.add(ouvinte);
    return () => ouvintes.delete(ouvinte);
  },
};

// Erro lançado pelo api.js quando a API não responde (as telas decidem se usam a cópia local ou a fila)
export class SemConexaoError extends Error {
  constructor() {
    super('Sem conexão com o servidor.');
  }
}
