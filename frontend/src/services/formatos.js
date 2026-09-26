// Formatação e datas usadas pelas telas

// 12.5 → "R$ 12,50"
export const dinheiro = (valor) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(valor) || 0);

// "2026-09-26" ou "2026-09-26T08:30:00" → "26/09/2026"
export const dataBR = (valor) => {
  const [ano, mes, dia] = String(valor).slice(0, 10).split('-');
  return `${dia}/${mes}/${ano}`;
};

// Data local (não UTC) no formato da API: "2026-09-26". dias = deslocamento (1 = amanhã).
export const dataISO = (dias = 0) => {
  const d = new Date();
  d.setDate(d.getDate() + dias);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

// "08:45:00" → "08:45"
export const hora = (valor) => String(valor ?? '').slice(0, 5);

// A janela de pedido do intervalo ainda está aberta nesta data? (a API confere de novo ao gravar)
export const janelaAberta = (intervalo, data) =>
  Boolean(intervalo) && new Date() < new Date(`${data}T${intervalo.fechamento}`);

// Enum da API → rótulo da tela
export const CATEGORIAS = { Salgado: 'Salgados', Doce: 'Doces', Bebida: 'Bebidas' };

// Domínio do e-mail dos alunos (o mesmo que a API exige no cadastro)
export const DOMINIO_ALUNO = 'aluno.cantina.test';

// ---------- Máscaras de entrada ----------
const digitos = (valor) => valor.replace(/\D/g, '');

// "12345678909" → "123.456.789-09" (vai formatando enquanto digita)
export const mascaraCpf = (valor) => digitos(valor).slice(0, 11)
  .replace(/(\d{3})(\d)/, '$1.$2')
  .replace(/(\d{3})(\d)/, '$1.$2')
  .replace(/(\d{3})(\d{1,2})$/, '$1-$2');

// "51900000001" → "(51) 90000-0001"; com 10 dígitos → "(51) 3333-4444"
export const mascaraTelefone = (valor) => {
  const d = digitos(valor).slice(0, 11);
  return d
    .replace(/(\d{2})(\d)/, '($1) $2')
    .replace(d.length > 10 ? /(\d{5})(\d)/ : /(\d{4})(\d)/, '$1-$2');
};

// E-mail sem espaços e em minúsculas
export const limparEmail = (valor) => valor.replace(/\s/g, '').toLowerCase();

// Padrões para a validação do navegador (a API confere o mesmo formato)
export const PADRAO_CPF = '\\d{3}\\.\\d{3}\\.\\d{3}-\\d{2}';
export const PADRAO_TELEFONE = '\\(\\d{2}\\) \\d{4,5}-\\d{4}';
