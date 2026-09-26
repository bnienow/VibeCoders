import { catalogoInicial, dataLocal, intervaloAberto, intervalos } from './catalogo';

const KEY = 'vibecoders-ui-demo-v3';
const listeners = new Set();
const round = (n) => Math.round((Number(n) + Number.EPSILON) * 100) / 100;
const uid = () => globalThis.crypto?.randomUUID?.() || String(Date.now() + Math.random());
const copy = (v) => structuredClone(v);
const currentMonth = () => dataLocal().slice(0, 7);
const previousMonth = () => {
  const d = new Date(); d.setMonth(d.getMonth() - 1);
  return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0');
};
const seed = () => ({
  usuarios: [
    { id: 'adulto-1', papel: 'Adulto', nome: 'Carla Andrade', email: 'carla@exemplo.test', cpf: '00000000000', telefone: '51999990000', saldo: 0 },
    { id: 'aluno-1', papel: 'Aluno', adultoId: 'adulto-1', nome: 'Marina Andrade', email: 'marina@aluno.cantina.test', turma: '8º ano', nascimento: '2012-04-12', restricoes: ['Lactose'], limiteDiario: 30, saldo: 42.50 },
    { id: 'aluno-2', papel: 'Aluno', adultoId: 'adulto-1', nome: 'Lucas Andrade', email: 'lucas@aluno.cantina.test', turma: '6º ano', nascimento: '2014-06-03', restricoes: [], limiteDiario: null, saldo: -240 },
    { id: 'admin-1', papel: 'Admin', nome: 'Cantina', email: 'cantina@exemplo.test', saldo: 0 },
  ],
  sessaoId: 'aluno-1', intervalo: 'manha', carrinho: {},
  itens: catalogoInicial,
  ofertas: { [dataLocal()]: { manha: catalogoInicial.map((i) => i.id), tarde: catalogoInicial.map((i) => i.id) } },
  pedidos: [
    { id: '1001', usuarioId: 'aluno-1', usuarioNome: 'Marina Andrade', data: dataLocal(), intervalo: 'manha', status: 'Aberto', tipoVenda: 'Antecipado', formaPagamento: 'Conta', codigoRetirada: 'A7K2', temAlertaAlergia: true, itens: [{ itemId: 1, nome: 'Pão de queijo', quantidade: 1, precoUnitario: 4.5, subtotal: 4.5 }], total: 4.5 },
    { id: '1002', usuarioId: 'aluno-2', usuarioNome: 'Lucas Andrade', data: dataLocal(), intervalo: 'manha', status: 'Aberto', tipoVenda: 'Antecipado', formaPagamento: 'Conta', codigoRetirada: 'B3M8', temAlertaAlergia: false, itens: [{ itemId: 2, nome: 'Coxinha', quantidade: 1, precoUnitario: 7, subtotal: 7 }], total: 7 },
  ],
  movimentos: [
    { id: 'm1', usuarioId: 'aluno-1', data: dataLocal(), tipo: 'Compra', descricao: 'Pedido 1001', valor: -4.5, saldoApos: 42.5, itens: [{ nome: 'Pão de queijo', quantidade: 1, precoUnitario: 4.5, subtotal: 4.5 }] },
    { id: 'm2', usuarioId: 'aluno-2', data: dataLocal(), tipo: 'Compra', descricao: 'Pedido 1002', valor: -7, saldoApos: -240, itens: [{ nome: 'Coxinha', quantidade: 1, precoUnitario: 7, subtotal: 7 }] },
    { id: 'm3', usuarioId: 'aluno-1', data: previousMonth() + '-18', tipo: 'Compra', descricao: 'Compra no balcão', valor: -10.5, saldoApos: 47, itens: [{ nome: 'Suco de laranja', quantidade: 1, precoUnitario: 6, subtotal: 6 }, { nome: 'Pão de queijo', quantidade: 1, precoUnitario: 4.5, subtotal: 4.5 }] },
  ],
  metodos: [{ id: 'metodo-1', tipo: 'Pix', apelido: 'Chave pessoal', ultimosDigitos: '', padrao: true }],
  fechamentos: [{ id: 'fechamento-1', adultoId: 'adulto-1', mesReferencia: previousMonth(), valorTotal: 18, status: 'Aberto', consumo: [{ nome: 'Marina Andrade', total: 10.5, itens: [{ nome: 'Suco de laranja', quantidade: 1, total: 6 }, { nome: 'Pão de queijo', quantidade: 1, total: 4.5 }] }, { nome: 'Lucas Andrade', total: 7.5, itens: [{ nome: 'Coxinha', quantidade: 1, total: 7.5 }] }] }],
});
function read() {
  try { const s = JSON.parse(localStorage.getItem(KEY)); if (s?.usuarios && s?.itens && s?.pedidos && s?.metodos && s?.fechamentos) return s; }
  catch { /* browser storage is optional */ }
  return seed();
}
let state = read();
function publish(next) {
  state = next;
  try { localStorage.setItem(KEY, JSON.stringify(state)); } catch { /* in-memory demo */ }
  listeners.forEach((listener) => listener());
}
function mutate(fn) { const next = copy(state); const result = fn(next); publish(next); return result; }
function requireUser(s, id) { const u = s.usuarios.find((v) => v.id === id); if (!u) throw new Error('Selecione um usuário válido.'); return u; }
function movement(s, u, tipo, valor, descricao, itens = []) {
  u.saldo = round(u.saldo + valor);
  s.movimentos.unshift({ id: uid(), usuarioId: u.id, data: dataLocal(), tipo, valor: round(valor), descricao, saldoApos: u.saldo, itens });
}
function lines(s, quantities) {
  const result = Object.entries(quantities).filter(([, q]) => Number(q) > 0).map(([key, q]) => {
    const item = s.itens.find((v) => String(v.id) === String(key));
    if (!item || !item.ativo) throw new Error('Item indisponível.');
    const quantidade = Math.floor(Number(q));
    if (!Number.isFinite(quantidade) || quantidade < 1) throw new Error('Quantidade inválida.');
    if (item.estoque < quantidade) throw new Error(item.nome + ' esgotou');
    return { itemId: item.id, nome: item.nome, quantidade, precoUnitario: item.preco, subtotal: round(item.preco * quantidade), alergenos: item.alergenos };
  });
  if (!result.length) throw new Error('Selecione pelo menos um item.');
  return result;
}
function validate(s, u, total, forma, extra = 0) {
  if (forma !== 'Conta' || u.papel !== 'Aluno') return;
  const spent = s.pedidos.filter((p) => p.usuarioId === u.id && p.data === dataLocal() && p.status !== 'Cancelado').reduce((n, p) => n + p.total, 0);
  if (u.limiteDiario != null && round(spent + extra + total) > Number(u.limiteDiario)) throw new Error('Limite diário de ' + money(u.limiteDiario) + ' atingido');
  if (round(u.saldo - total) < -250) throw new Error('Limite de R$ 250,00 atingido — somente à vista');
}
export const money = (n) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(n) || 0);
export const disponivelNoIntervalo = (s, item, intervalo, date = dataLocal()) => Boolean(item?.ativo && item.estoque > 0 && s.ofertas?.[date]?.[intervalo]?.includes(item.id));
export const statusPedido = (p) => p.status === 'Aberto' && !intervaloAberto(p.intervalo, p.data) ? 'Confirmado' : p.status;
export const displayDate = (v) => { const [y, m, d] = String(v).slice(0, 10).split('-'); return d && m && y ? d + '/' + m + '/' + y : String(v); };
export const cantinaService = {
  snapshot: () => state,
  subscribe(listener) { listeners.add(listener); return () => listeners.delete(listener); },
  usuarioAtual(s = state) { return s.usuarios.find((u) => u.id === s.sessaoId) || null; },
  itensDoCarrinho(s = state) { return Object.entries(s.carrinho).map(([id, q]) => { const i = s.itens.find((v) => String(v.id) === id); return i ? { ...i, quantidade: q } : null; }).filter(Boolean); },
  totalDoCarrinho(s = state) { return round(this.itensDoCarrinho(s).reduce((n, i) => n + i.preco * i.quantidade, 0)); },
  entrar(email, senha) {
    const u = state.usuarios.find((v) => v.email.toLowerCase() === email.trim().toLowerCase());
    if (!u || senha !== 'senha123') throw new Error('Credenciais demonstrativas inválidas.');
    mutate((s) => { s.sessaoId = u.id; s.carrinho = {}; });
    return u;
  },
  sair() { mutate((s) => { s.sessaoId = null; s.carrinho = {}; }); },
  cadastrarAdulto(data) {
    if (!data.nome?.trim() || !/^[^@]+@[^@]+\.[^@]+$/.test(data.email || '') || !/^\d{11}$/.test((data.cpf || '').replace(/\D/g, '')) || (data.telefone || '').replace(/\D/g, '').length < 10 || (data.senha || '').length < 6) throw new Error('Confira nome, e-mail, CPF e senha (mínimo 6 caracteres).');
    if (state.usuarios.some((u) => u.email === data.email.trim().toLowerCase())) throw new Error('E-mail já cadastrado.');
    return mutate((s) => { const u = { id: uid(), papel: 'Adulto', nome: data.nome.trim(), email: data.email.trim().toLowerCase(), cpf: data.cpf, telefone: data.telefone, saldo: 0 }; s.usuarios.push(u); s.sessaoId = u.id; return u; });
  },
  cadastrarAluno(data) {
    const adult = this.usuarioAtual(); if (adult?.papel !== 'Adulto') throw new Error('Apenas o responsável logado cadastra o aluno.');
    if (!data.nome?.trim() || !data.turma?.trim() || !data.nascimento || !/^[^@]+@aluno\.cantina\.test$/i.test(data.email || '') || (data.senha || '').length < 6) throw new Error('Confira os dados e use o e-mail institucional do aluno.');
    if (state.usuarios.some((u) => u.email === data.email.trim().toLowerCase())) throw new Error('E-mail já cadastrado.');
    return mutate((s) => { const u = { id: uid(), papel: 'Aluno', adultoId: adult.id, nome: data.nome.trim(), email: data.email.trim().toLowerCase(), turma: data.turma.trim(), nascimento: data.nascimento, restricoes: (data.restricoes || '').split(',').map((x) => x.trim()).filter(Boolean), limiteDiario: null, saldo: 0 }; s.usuarios.push(u); return u; });
  },
  escolherIntervalo(chave) { if (intervalos[chave]) mutate((s) => { s.intervalo = chave; s.carrinho = {}; }); },
  definirQuantidade(itemId, q) { mutate((s) => { const i = s.itens.find((v) => String(v.id) === String(itemId)); if (!disponivelNoIntervalo(s, i, s.intervalo) || i.categoria === 'Combos') return; const n = Math.min(i.estoque, Math.max(0, Math.floor(Number(q) || 0))); if (n) s.carrinho[i.id] = n; else delete s.carrinho[i.id]; }); },
  limparCarrinho() { mutate((s) => { s.carrinho = {}; }); },
  confirmarPedido(forma = 'Conta') {
    const u = this.usuarioAtual(); if (u?.papel !== 'Aluno') throw new Error('Selecione uma conta de aluno.');
    if (!intervaloAberto(state.intervalo)) throw new Error('Pedidos para este intervalo já fecharam');
    const orderLines = lines(state, state.carrinho);
    for (const l of orderLines) { const item = state.itens.find((i) => i.id === l.itemId); if (!disponivelNoIntervalo(state, item, state.intervalo) || item.categoria === 'Combos') throw new Error(item.nome + ' não está no cardápio deste intervalo'); }
    if (state.pedidos.some((p) => p.usuarioId === u.id && p.data === dataLocal() && p.intervalo === state.intervalo && p.tipoVenda === 'Antecipado' && p.status !== 'Cancelado')) throw new Error('Você já tem um pedido neste intervalo. Altere o pedido existente.');
    const total = round(orderLines.reduce((n, l) => n + l.subtotal, 0)); validate(state, u, total, forma);
    return mutate((s) => {
      const user = requireUser(s, u.id), id = String(Date.now());
      const order = { id, usuarioId: u.id, usuarioNome: u.nome, data: dataLocal(), intervalo: s.intervalo, status: 'Aberto', tipoVenda: 'Antecipado', formaPagamento: forma, codigoRetirada: Math.random().toString(36).slice(2, 6).toUpperCase(), temAlertaAlergia: orderLines.some((l) => l.alergenos.some((a) => user.restricoes?.some((r) => r.toLowerCase() === a.toLowerCase()))), total, itens: orderLines.map(({ alergenos, ...rest }) => rest) };
      orderLines.forEach((l) => { s.itens.find((i) => i.id === l.itemId).estoque -= l.quantidade; });
      if (forma === 'Conta') movement(s, user, 'Compra', -total, 'Pedido ' + id, order.itens);
      s.pedidos.unshift(order); s.carrinho = {}; return order;
    });
  },
  alterarPedido(orderId, quantities) {
    const u = this.usuarioAtual(), old = state.pedidos.find((p) => p.id === orderId && p.usuarioId === u?.id && p.tipoVenda === 'Antecipado');
    if (!old || old.status !== 'Aberto' || !intervaloAberto(old.intervalo, old.data)) throw new Error('O prazo de alteração terminou.');
    const available = copy(state); old.itens.forEach((l) => { available.itens.find((i) => i.id === l.itemId).estoque += l.quantidade; });
    const nextLines = lines(available, quantities);
    for (const l of nextLines) if (!disponivelNoIntervalo(available, available.itens.find((i) => i.id === l.itemId), old.intervalo, old.data)) throw new Error(l.nome + ' não está no cardápio deste intervalo');
    const total = round(nextLines.reduce((n, l) => n + l.subtotal, 0));
    const delta = round(total - old.total);
    validate(state, u, delta, old.formaPagamento);
    return mutate((s) => {
      const p = s.pedidos.find((v) => v.id === orderId), user = requireUser(s, u.id);
      p.itens.forEach((l) => { s.itens.find((i) => i.id === l.itemId).estoque += l.quantidade; });
      nextLines.forEach((l) => { s.itens.find((i) => i.id === l.itemId).estoque -= l.quantidade; });
      p.itens = nextLines.map(({ alergenos, ...rest }) => rest); p.total = total;
      p.temAlertaAlergia = nextLines.some((l) => l.alergenos.some((a) => user.restricoes?.some((r) => r.toLowerCase() === a.toLowerCase())));
      if (p.formaPagamento === 'Conta' && delta) movement(s, user, delta > 0 ? 'Compra' : 'Estorno', -delta, 'Alteração do pedido ' + p.id, p.itens);
      return p;
    });
  },
  cancelarPedido(orderId) {
    const u = this.usuarioAtual(), p = state.pedidos.find((v) => v.id === orderId && v.usuarioId === u?.id);
    if (!p || p.status !== 'Aberto' || !intervaloAberto(p.intervalo, p.data)) throw new Error('O prazo de cancelamento terminou.');
    mutate((s) => { const order = s.pedidos.find((v) => v.id === orderId); order.status = 'Cancelado'; order.itens.forEach((l) => { s.itens.find((i) => i.id === l.itemId).estoque += l.quantidade; }); if (order.formaPagamento === 'Conta') movement(s, requireUser(s, u.id), 'Estorno', order.total, 'Cancelamento ' + orderId); });
  },
  entregarPedido(orderId) {
    mutate((s) => { const p = s.pedidos.find((v) => v.id === orderId && v.tipoVenda === 'Antecipado'); if (!p || p.status === 'Cancelado' || p.status === 'Entregue') throw new Error('Pedido não disponível para entrega.'); p.status = 'Entregue'; p.entregueEm = new Date().toISOString(); });
  },
  venderBalcao(userId, quantities, forma = 'Conta') {
    const u = requireUser(state, userId); if (!['Aluno', 'Adulto'].includes(u.papel)) throw new Error('Selecione aluno ou responsável.');
    const orderLines = lines(state, quantities), total = round(orderLines.reduce((n, l) => n + l.subtotal, 0));
    validate(state, u, total, forma);
    return mutate((s) => {
      const user = requireUser(s, userId), id = String(Date.now());
      const p = { id, usuarioId: userId, usuarioNome: u.nome, data: dataLocal(), intervalo: null, status: 'Entregue', tipoVenda: 'Balcao', formaPagamento: forma, codigoRetirada: '', temAlertaAlergia: orderLines.some((l) => l.alergenos.some((a) => user.restricoes?.some((r) => r.toLowerCase() === a.toLowerCase()))), total, itens: orderLines.map(({ alergenos, ...rest }) => rest) };
      orderLines.forEach((l) => { s.itens.find((i) => i.id === l.itemId).estoque -= l.quantidade; });
      if (forma === 'Conta') movement(s, user, 'Compra', -total, 'Compra no balcão ' + id, p.itens);
      s.pedidos.unshift(p); return p;
    });
  },
  adicionarCredito(userId, value, methodId) {
    const n = Number(value); if (!Number.isFinite(n) || n < 1 || n > 1000) throw new Error('Informe um valor de R$ 1 a R$ 1.000.');
    if (methodId && !state.metodos.some((m) => m.id === methodId)) throw new Error('Escolha um método válido.');
    mutate((s) => movement(s, requireUser(s, userId), 'Credito', n, 'Crédito demonstrativo'));
  },
  definirLimite(userId, value) {
    if (value !== null && (!Number.isFinite(Number(value)) || Number(value) <= 0)) throw new Error('Informe um limite maior que zero.');
    mutate((s) => { requireUser(s, userId).limiteDiario = value === null ? null : round(value); });
  },
  definirRestricoes(userId, text) { mutate((s) => { requireUser(s, userId).restricoes = text.split(',').map((x) => x.trim()).filter(Boolean); }); },
  salvarMetodo(data) {
    if (!['Pix', 'Cartao'].includes(data.tipo) || !data.apelido.trim() || (data.tipo === 'Cartao' && !/^\d{4}$/.test(data.ultimosDigitos))) throw new Error('Informe tipo, apelido e, para cartão, os quatro últimos dígitos fictícios.');
    return mutate((s) => { if (data.padrao) s.metodos.forEach((m) => { m.padrao = false; }); const m = { id: uid(), tipo: data.tipo, apelido: data.apelido.trim(), ultimosDigitos: data.tipo === 'Cartao' ? data.ultimosDigitos : '', padrao: Boolean(data.padrao) || !s.metodos.length }; s.metodos.push(m); return m; });
  },
  definirMetodoPadrao(id) { mutate((s) => { if (!s.metodos.some((m) => m.id === id)) throw new Error('Método inválido.'); s.metodos.forEach((m) => { m.padrao = m.id === id; }); }); },
  pagarFechamento(id, methodId) {
    if (!state.metodos.some((m) => m.id === methodId)) throw new Error('Selecione um método de pagamento.');
    mutate((s) => { const f = s.fechamentos.find((v) => v.id === id); if (!f || f.status !== 'Aberto') throw new Error('Fechamento indisponível.'); f.status = 'Pago'; f.pagoEm = new Date().toISOString(); let saldo = f.valorTotal; const family = s.usuarios.filter((u) => u.id === f.adultoId || u.adultoId === f.adultoId).filter((u) => u.saldo < 0).sort((a, b) => a.saldo - b.saldo); family.forEach((u) => { const amount = Math.min(saldo, -u.saldo); if (amount) movement(s, u, 'Pagamento', amount, 'Fechamento ' + f.mesReferencia); saldo = round(saldo - amount); }); if (saldo) movement(s, requireUser(s, f.adultoId), 'Pagamento', saldo, 'Crédito do fechamento ' + f.mesReferencia); });
  },
  salvarItem(data) {
    const preco = Number(data.preco), estoque = Number(data.estoque);
    if (!data.nome?.trim() || !data.descricao?.trim() || !['Salgados', 'Doces', 'Bebidas'].includes(data.categoria) || !Number.isFinite(preco) || preco <= 0 || !Number.isInteger(estoque) || estoque < 0) throw new Error('Confira nome, descrição, categoria, preço e estoque.');
    return mutate((s) => { const i = data.id ? s.itens.find((v) => v.id === data.id) : { id: Math.max(0, ...s.itens.map((v) => v.id)) + 1, ativo: true, disponivel: true }; if (!i) throw new Error('Item não encontrado.'); Object.assign(i, { nome: data.nome.trim(), descricao: data.descricao.trim(), categoria: data.categoria, preco: round(preco), estoque, alergenos: (data.alergenos || '').split(',').map((x) => x.trim()).filter(Boolean) }); if (!data.id) s.itens.push(i); return i; });
  },
  definirOferta(id, intervalo, disponivel) {
    if (!intervalos[intervalo] || !state.itens.some((i) => i.id === id)) throw new Error('Item ou intervalo inválido.');
    mutate((s) => { const day = s.ofertas[dataLocal()] || (s.ofertas[dataLocal()] = { manha:[], tarde:[] }); const ids = new Set(day[intervalo] || []); if (disponivel) ids.add(id); else ids.delete(id); day[intervalo] = Array.from(ids); });
  },
  desativarItem(id) { mutate((s) => { const i = s.itens.find((v) => v.id === id); if (!i) throw new Error('Item não encontrado.'); i.ativo = false; }); },
  atualizarPerfil(data) {
    const u = this.usuarioAtual(); if (!u) throw new Error('Entre para editar o perfil.');
    if (!data.nome?.trim() || !/^[^@]+@[^@]+\.[^@]+$/.test(data.email || '')) throw new Error('Informe nome e e-mail válidos.');
    mutate((s) => { const user = requireUser(s, u.id); user.nome = data.nome.trim(); user.email = data.email.trim(); });
  },
};
