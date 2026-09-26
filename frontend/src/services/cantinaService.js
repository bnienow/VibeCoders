import { catalogoInicial, dataDeslocada, dataLocal, intervaloAberto } from './catalogo';

const CHAVE = 'cantina-do-patio-prototipo-v1';
const HASH_SENHA_DEMO = '55a5e9e78207b4df8699d60886fa070079463547b095d1a05bc719bb4e6cd251';
// Senha das contas fictícias: senha123. Este serviço é um protótipo local.
// Para integrar o backend, substitua suas operações por fetch('/api/...').
const ouvintes = new Set();

function inicial() {
  return {
    usuarios: [
      { id: 'adulto-demo', papel: 'Adulto', nome: 'Carla Andrade', email: 'carla@email.test', senhaHash: HASH_SENHA_DEMO, cpf: '12345678909', telefone: '11998765432', saldo: 0 },
      { id: 'aluno-demo', papel: 'Aluno', adultoId: 'adulto-demo', nome: 'Marina Andrade', email: 'marina@aluno.cantina.test', senhaHash: HASH_SENHA_DEMO, turma: '8º ano', nascimento: '2012-04-12', restricoes: '', limiteDiario: null, saldo: 42.5, avisos: true },
    ],
    sessaoId: null, carrinho: {}, intervalo: 'manha', itens: catalogoInicial,
    pedidos: [{ id: 'CP-1037', usuarioId: 'aluno-demo', data: dataDeslocada(-1), intervalo: 'manha', status: 'Entregue', formaPagamento: 'Conta', codigoRetirada: 'A7K2', itens: [{ id: 1, nome: 'Pão de queijo', quantidade: 1, preco: 4.5 }, { id: 25, nome: 'Suco de laranja', quantidade: 1, preco: 6 }], total: 10.5 }],
    movimentos: [{ id: 'mov-demo', usuarioId: 'aluno-demo', data: dataDeslocada(-1), tipo: 'Compra', descricao: 'Pedido CP-1037', valor: -10.5, saldoApos: 42.5 }],
  };
}

function carregar() {
  try {
    const salvo = JSON.parse(localStorage.getItem(CHAVE));
    if (salvo?.usuarios && salvo?.itens && salvo?.pedidos) return salvo;
  } catch { /* O protótipo pode funcionar apenas em memória. */ }
  return inicial();
}

let estado = carregar();
function publicar(proximo) {
  estado = proximo;
  try { localStorage.setItem(CHAVE, JSON.stringify(estado)); } catch { /* Mantém em memória. */ }
  ouvintes.forEach((ouvinte) => ouvinte());
}
function copiar() { return structuredClone(estado); }
function id() { return crypto.randomUUID?.() ?? `${Date.now()}-${Math.random()}`; }
async function hash(senha) {
  const digest = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(senha));
  return [...new Uint8Array(digest)].map((byte) => byte.toString(16).padStart(2, '0')).join('');
}
function validarEmail(email) { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email); }
function usuarioAtual(dados = estado) { return dados.usuarios.find((usuario) => usuario.id === dados.sessaoId) ?? null; }
function totalDoCarrinho(dados = estado) {
  return Object.entries(dados.carrinho).reduce((total, [itemId, quantidade]) => {
    const item = dados.itens.find((atual) => atual.id === Number(itemId));
    return total + (item?.preco ?? 0) * quantidade;
  }, 0);
}
function itensDoCarrinho(dados = estado) {
  return Object.entries(dados.carrinho).map(([itemId, quantidade]) => {
    const item = dados.itens.find((atual) => atual.id === Number(itemId));
    return item ? { ...item, quantidade } : null;
  }).filter(Boolean);
}
function gastoHoje(dados, usuarioId) {
  return dados.pedidos.filter((pedido) => pedido.usuarioId === usuarioId && pedido.data === dataLocal() && pedido.status !== 'Cancelado').reduce((soma, pedido) => soma + pedido.total, 0);
}

export const cantinaService = {
  snapshot: () => estado,
  subscribe(ouvinte) { ouvintes.add(ouvinte); return () => ouvintes.delete(ouvinte); },
  usuarioAtual, totalDoCarrinho, itensDoCarrinho,

  async entrar(email, senha) {
    const usuario = estado.usuarios.find((atual) => atual.email.toLowerCase() === email.trim().toLowerCase());
    if (!usuario || usuario.senhaHash !== await hash(senha)) throw new Error('E-mail ou senha inválidos.');
    publicar({ ...estado, sessaoId: usuario.id, carrinho: {} });
    return usuario;
  },
  sair() { publicar({ ...estado, sessaoId: null, carrinho: {} }); },

  async cadastrarAdulto(campos) {
    const nome = campos.nome.trim(), email = campos.email.trim().toLowerCase();
    const cpf = campos.cpf.replace(/\D/g, ''), telefone = campos.telefone.replace(/\D/g, '');
    if (nome.length < 3) throw new Error('Informe o nome completo.');
    if (!validarEmail(email)) throw new Error('Informe um e-mail válido.');
    if (cpf.length !== 11) throw new Error('O CPF deve ter 11 dígitos.');
    if (telefone.length < 10) throw new Error('Informe um telefone válido.');
    if (campos.senha.length < 6) throw new Error('A senha deve ter pelo menos 6 caracteres.');
    if (estado.usuarios.some((usuario) => usuario.email === email)) throw new Error('E-mail já cadastrado.');
    const novo = { id: id(), papel: 'Adulto', nome, email, cpf, telefone, senhaHash: await hash(campos.senha), saldo: 0 };
    publicar({ ...estado, usuarios: [...estado.usuarios, novo], sessaoId: novo.id, carrinho: {} });
    return novo;
  },

  async cadastrarAluno(campos) {
    const adulto = usuarioAtual();
    if (adulto?.papel !== 'Adulto') throw new Error('Entre com a conta do responsável para cadastrar um aluno.');
    const nome = campos.nome.trim(), email = campos.email.trim().toLowerCase();
    if (nome.length < 3) throw new Error('Informe o nome completo.');
    if (!validarEmail(email) || !email.endsWith('@aluno.cantina.test')) throw new Error('No protótipo, use um e-mail @aluno.cantina.test.');
    if (!campos.turma.trim()) throw new Error('Informe a turma.');
    if (!campos.nascimento) throw new Error('Informe a data de nascimento.');
    if (campos.senha.length < 6) throw new Error('A senha deve ter pelo menos 6 caracteres.');
    if (estado.usuarios.some((usuario) => usuario.email === email)) throw new Error('E-mail já cadastrado.');
    const novo = { id: id(), papel: 'Aluno', adultoId: adulto.id, nome, email, turma: campos.turma.trim(), nascimento: campos.nascimento, restricoes: campos.restricoes.trim(), limiteDiario: null, saldo: 0, avisos: true, senhaHash: await hash(campos.senha) };
    publicar({ ...estado, usuarios: [...estado.usuarios, novo] });
    return novo;
  },

  escolherIntervalo(intervalo) {
    if (['manha', 'tarde'].includes(intervalo)) publicar({ ...estado, intervalo });
  },
  definirQuantidade(itemId, quantidade) {
    const item = estado.itens.find((atual) => atual.id === Number(itemId));
    if (!item?.ativo || !item.disponivel || item.estoque < 1) return;
    const proximo = copiar();
    const valor = Math.max(0, Math.min(item.estoque, Math.trunc(Number(quantidade) || 0)));
    if (valor === 0) delete proximo.carrinho[item.id]; else proximo.carrinho[item.id] = valor;
    publicar(proximo);
  },
  limparCarrinho() { publicar({ ...estado, carrinho: {} }); },

  confirmarPedido(formaPagamento) {
    const usuario = usuarioAtual();
    if (usuario?.papel !== 'Aluno') throw new Error('Entre com uma conta de aluno para fazer um pedido.');
    if (!['Conta', 'AVista'].includes(formaPagamento)) throw new Error('Escolha uma forma de pagamento.');
    if (!intervaloAberto(estado.intervalo)) throw new Error('Pedidos para este intervalo já fecharam.');
    const linhas = itensDoCarrinho();
    if (!linhas.length) throw new Error('Escolha pelo menos um item.');
    for (const item of linhas) if (!item.ativo || !item.disponivel || item.estoque < item.quantidade) throw new Error(`${item.nome} esgotou.`);
    const existente = estado.pedidos.some((pedido) => pedido.usuarioId === usuario.id && pedido.data === dataLocal() && pedido.intervalo === estado.intervalo && pedido.status !== 'Cancelado');
    if (existente) throw new Error('Você já possui um pedido neste intervalo.');
    const total = totalDoCarrinho();
    if (formaPagamento === 'Conta') {
      if (usuario.limiteDiario != null && gastoHoje(estado, usuario.id) + total > usuario.limiteDiario) throw new Error('Limite diário definido pelo responsável atingido.');
      if (usuario.saldo - total < -250) throw new Error('Limite de R$ 250,00 atingido — somente à vista.');
    }
    const proximo = copiar();
    const aluno = proximo.usuarios.find((atual) => atual.id === usuario.id);
    const pedido = { id: `CP-${String(Date.now()).slice(-6)}`, usuarioId: usuario.id, data: dataLocal(), intervalo: proximo.intervalo, status: 'Confirmado', formaPagamento, codigoRetirada: Math.random().toString(36).slice(2, 6).toUpperCase(), itens: linhas.map((item) => ({ id: item.id, nome: item.nome, quantidade: item.quantidade, preco: item.preco })), total };
    for (const linha of linhas) proximo.itens.find((atual) => atual.id === linha.id).estoque -= linha.quantidade;
    if (formaPagamento === 'Conta') {
      aluno.saldo = Math.round((aluno.saldo - total) * 100) / 100;
      proximo.movimentos.unshift({ id: id(), usuarioId: aluno.id, data: dataLocal(), tipo: 'Compra', descricao: `Pedido ${pedido.id}`, valor: -total, saldoApos: aluno.saldo });
    }
    proximo.pedidos.unshift(pedido); proximo.carrinho = {}; publicar(proximo); return pedido;
  },

  cancelarPedido(pedidoId) {
    const usuario = usuarioAtual();
    const pedido = estado.pedidos.find((atual) => atual.id === pedidoId && atual.usuarioId === usuario?.id);
    if (!pedido || pedido.status !== 'Confirmado') throw new Error('Este pedido não pode ser cancelado.');
    if (!intervaloAberto(pedido.intervalo, pedido.data)) throw new Error('O prazo de cancelamento deste intervalo já terminou.');
    const proximo = copiar();
    const alterado = proximo.pedidos.find((atual) => atual.id === pedidoId); alterado.status = 'Cancelado';
    for (const linha of alterado.itens) { const item = proximo.itens.find((atual) => atual.id === linha.id); if (item) item.estoque += linha.quantidade; }
    if (alterado.formaPagamento === 'Conta') {
      const aluno = proximo.usuarios.find((atual) => atual.id === usuario.id);
      aluno.saldo = Math.round((aluno.saldo + alterado.total) * 100) / 100;
      proximo.movimentos.unshift({ id: id(), usuarioId: aluno.id, data: dataLocal(), tipo: 'Estorno', descricao: `Cancelamento ${pedidoId}`, valor: alterado.total, saldoApos: aluno.saldo });
    }
    publicar(proximo);
  },

  atualizarPerfil({ nome, email, avisos }) {
    const atual = usuarioAtual(); if (!atual) throw new Error('Entre para editar o perfil.');
    const nomeLimpo = nome.trim(), emailLimpo = email.trim().toLowerCase();
    if (nomeLimpo.length < 3) throw new Error('Informe o nome completo.');
    if (!validarEmail(emailLimpo)) throw new Error('Informe um e-mail válido.');
    if (atual.papel === 'Aluno' && !emailLimpo.endsWith('@aluno.cantina.test')) throw new Error('Use o e-mail institucional do protótipo.');
    if (estado.usuarios.some((usuario) => usuario.id !== atual.id && usuario.email === emailLimpo)) throw new Error('E-mail já cadastrado.');
    const proximo = copiar(); const usuario = proximo.usuarios.find((item) => item.id === atual.id);
    usuario.nome = nomeLimpo; usuario.email = emailLimpo; usuario.avisos = Boolean(avisos); publicar(proximo);
  },
};
