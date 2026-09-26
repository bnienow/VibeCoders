export const intervalos = {
  manha: { nome: 'Manhã', inicio: '09:00', fechamento: '08:45' },
  tarde: { nome: 'Tarde', inicio: '15:30', fechamento: '15:15' },
};

const dados = [
  ['Salgados', 'Pão de queijo', 'Porção com 3 unidades, assado na hora', 4.5, 'Lactose'],
  ['Salgados', 'Coxinha', 'Coxinha de frango com catupiry', 7, 'Gluten,Lactose'],
  ['Salgados', 'Pastel de carne', 'Pastel frito de carne moída', 7.5, 'Gluten'],
  ['Salgados', 'Pastel de queijo', 'Pastel frito de queijo muçarela', 7.5, 'Gluten,Lactose'],
  ['Salgados', 'Esfiha de carne', 'Esfiha aberta de carne temperada', 6.5, 'Gluten'],
  ['Salgados', 'Enroladinho de salsicha', 'Massa assada com salsicha', 6, 'Gluten'],
  ['Salgados', 'Empada de frango', 'Empada de massa podre com frango', 7, 'Gluten,Lactose,Ovo'],
  ['Salgados', 'Quibe', 'Quibe frito de carne e trigo', 6.5, 'Gluten'],
  ['Salgados', 'Misto quente', 'Pão de forma com presunto e queijo na chapa', 8, 'Gluten,Lactose'],
  ['Salgados', 'Sanduíche natural', 'Pão integral com frango, cenoura e requeijão', 9.5, 'Gluten,Lactose'],
  ['Salgados', 'Pão de batata', 'Pão de batata recheado com requeijão', 6.5, 'Gluten,Lactose'],
  ['Salgados', 'Tapioca de queijo', 'Tapioca com queijo coalho', 8.5, 'Lactose'],
  ['Doces', 'Brigadeiro', 'Brigadeiro tradicional', 3, 'Lactose'],
  ['Doces', 'Bolo de cenoura', 'Fatia com cobertura de chocolate', 5.5, 'Gluten,Lactose,Ovo'],
  ['Doces', 'Bolo de chocolate', 'Fatia de bolo de chocolate', 5.5, 'Gluten,Lactose,Ovo'],
  ['Doces', 'Cookie', 'Cookie com gotas de chocolate', 4.5, 'Gluten,Lactose,Ovo'],
  ['Doces', 'Paçoca', 'Paçoca de amendoim', 2, 'Amendoim'],
  ['Doces', 'Pé de moleque', 'Doce de amendoim com rapadura', 2.5, 'Amendoim'],
  ['Doces', 'Brownie', 'Brownie de chocolate meio amargo', 6, 'Gluten,Lactose,Ovo'],
  ['Doces', 'Alfajor', 'Alfajor com doce de leite', 5, 'Gluten,Lactose'],
  ['Doces', 'Salada de frutas', 'Copo de 300 ml com frutas da estação', 7, ''],
  ['Doces', 'Barra de cereal', 'Barra de cereal com mel', 3.5, 'Gluten,Soja'],
  ['Bebidas', 'Água mineral', 'Garrafa de 500 ml', 3, ''],
  ['Bebidas', 'Água com gás', 'Garrafa de 500 ml', 3.5, ''],
  ['Bebidas', 'Suco de laranja', 'Copo de 300 ml, natural', 6, ''],
  ['Bebidas', 'Suco de uva', 'Copo de 300 ml, integral', 6, ''],
  ['Bebidas', 'Suco de maracujá', 'Copo de 300 ml, natural', 6, ''],
  ['Bebidas', 'Refrigerante lata', 'Lata de 350 ml', 5.5, ''],
  ['Bebidas', 'Chá gelado', 'Garrafa de 300 ml, pêssego', 5, ''],
  ['Bebidas', 'Achocolatado', 'Caixinha de 200 ml', 4.5, 'Lactose,Soja'],
  ['Bebidas', 'Iogurte', 'Iogurte de morango, 170 g', 5, 'Lactose'],
  ['Bebidas', 'Vitamina de banana', 'Copo de 300 ml, batida com leite', 7.5, 'Lactose'],
  ['Bebidas', 'Leite', 'Copo de 200 ml', 3.5, 'Lactose'],
  ['Bebidas', 'Café com leite', 'Copo de 200 ml', 4, 'Lactose'],
  ['Combos', 'Combo Lanche', 'Pão de queijo + suco de laranja', 9.5, 'Lactose'],
  ['Combos', 'Combo Coxinha', 'Coxinha + refrigerante lata', 11, 'Gluten,Lactose'],
  ['Combos', 'Combo Misto', 'Misto quente + achocolatado', 11, 'Gluten,Lactose,Soja'],
  ['Combos', 'Combo Doce', 'Brigadeiro + água mineral', 5.5, 'Lactose'],
];

export const catalogoInicial = dados.map(([categoria, nome, descricao, preco, alergenos], indice) => ({
  id: indice + 1, categoria, nome, descricao, preco,
  alergenos: alergenos ? alergenos.split(',') : [],
  estoque: 20, ativo: true, disponivel: true,
}));

export const dinheiro = (valor) => new Intl.NumberFormat('pt-BR', {
  style: 'currency', currency: 'BRL',
}).format(valor);

export function dataLocal(data = new Date()) {
  return `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, '0')}-${String(data.getDate()).padStart(2, '0')}`;
}

export function dataDeslocada(dias) {
  const data = new Date(); data.setDate(data.getDate() + dias); return dataLocal(data);
}

export function intervaloAberto(chave, data = dataLocal()) {
  const intervalo = intervalos[chave];
  if (!intervalo || data !== dataLocal()) return false;
  const [hora, minuto] = intervalo.fechamento.split(':').map(Number);
  const agora = new Date();
  return agora.getHours() * 60 + agora.getMinutes() < hora * 60 + minuto;
}

export function minutosRestantes(chave) {
  const intervalo = intervalos[chave]; if (!intervalo) return 0;
  const [hora, minuto] = intervalo.fechamento.split(':').map(Number);
  const agora = new Date();
  return Math.max(0, hora * 60 + minuto - agora.getHours() * 60 - agora.getMinutes());
}
