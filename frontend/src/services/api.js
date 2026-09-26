// Único ponto de acesso à API. O cookie de sessão vai junto sozinho: front e API estão
// na mesma origem graças ao proxy do Vite (/api → backend).
// Se a API responder erro, lança uma exceção com a mensagem dela, pronta para mostrar na tela.
export async function api(rota, { metodo = 'GET', corpo } = {}) {
  const resposta = await fetch(`/api${rota}`, {
    method: metodo,
    headers: corpo ? { 'Content-Type': 'application/json' } : undefined,
    body: corpo ? JSON.stringify(corpo) : undefined,
  });

  const texto = await resposta.text();
  if (!resposta.ok) throw new Error(mensagemDeErro(texto, resposta.status));

  return texto ? JSON.parse(texto) : null;
}

// A API responde erro de dois jeitos:
// - regra de negócio: texto puro ("Limite de R$ 250,00 atingido — somente à vista")
// - validação do ASP.NET: JSON com a lista de campos inválidos em "errors"
function mensagemDeErro(texto, status) {
  try {
    const json = JSON.parse(texto);
    if (json.errors) return Object.values(json.errors).flat().join(' ');
    return json.title ?? texto;
  } catch {
    if (texto) return texto;
  }

  if (status === 401) return 'Sua sessão expirou. Entre novamente.';
  if (status === 403) return 'Você não tem permissão para isso.';
  return 'Não foi possível falar com o servidor. Tente de novo.';
}
