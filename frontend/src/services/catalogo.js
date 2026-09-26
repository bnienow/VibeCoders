export const intervalos = {
  manha: { nome: 'Manhã', inicio: '09:00', fechamento: '08:45' },
  tarde: { nome: 'Tarde', inicio: '15:30', fechamento: '15:15' },
};
const dados = [
  ['Salgados', 'Pão de queijo', 'Porção com 3 unidades', 4.5, 'Lactose'],
  ['Salgados', 'Coxinha', 'Frango e catupiry', 7, 'Gluten,Lactose'],
  ['Salgados', 'Pastel de carne', 'Massa crocante e carne', 7.5, 'Gluten'],
  ['Salgados', 'Pastel de queijo', 'Queijo muçarela', 7.5, 'Gluten,Lactose'],
  ['Salgados', 'Esfiha de carne', 'Assada na hora', 6.5, 'Gluten'],
  ['Salgados', 'Enroladinho', 'Massa assada e salsicha', 6, 'Gluten'],
  ['Salgados', 'Empada de frango', 'Massa e frango', 7, 'Gluten,Lactose,Ovo'],
  ['Salgados', 'Quibe', 'Carne e trigo', 6.5, 'Gluten'],
  ['Salgados', 'Misto quente', 'Presunto e queijo', 8, 'Gluten,Lactose'],
  ['Salgados', 'Sanduíche natural', 'Frango e cenoura', 9.5, 'Gluten,Lactose'],
  ['Salgados', 'Pão de batata', 'Recheio de requeijão', 6.5, 'Gluten,Lactose'],
  ['Salgados', 'Tapioca', 'Recheio de queijo', 8.5, 'Lactose'],
  ['Doces', 'Brigadeiro', 'Chocolate tradicional', 3, 'Lactose'],
  ['Doces', 'Bolo de cenoura', 'Fatia com cobertura', 5.5, 'Gluten,Lactose,Ovo'],
  ['Doces', 'Bolo de chocolate', 'Fatia com cobertura', 5.5, 'Gluten,Lactose,Ovo'],
  ['Doces', 'Cookie', 'Gotas de chocolate', 4.5, 'Gluten,Lactose,Ovo'],
  ['Doces', 'Paçoca', 'Amendoim', 2, 'Amendoim'],
  ['Doces', 'Pé de moleque', 'Amendoim e rapadura', 2.5, 'Amendoim'],
  ['Doces', 'Brownie', 'Chocolate meio amargo', 6, 'Gluten,Lactose,Ovo'],
  ['Doces', 'Alfajor', 'Recheio de doce de leite', 5, 'Gluten,Lactose'],
  ['Doces', 'Salada de frutas', 'Copo de 300 ml', 7, ''],
  ['Doces', 'Barra de cereal', 'Mel e aveia', 3.5, 'Gluten,Soja'],
  ['Bebidas', 'Água mineral', 'Garrafa de 500 ml', 3, ''],
  ['Bebidas', 'Água com gás', 'Garrafa de 500 ml', 3.5, ''],
  ['Bebidas', 'Suco de laranja', 'Copo de 300 ml', 6, ''],
  ['Bebidas', 'Suco de uva', 'Copo de 300 ml', 6, ''],
  ['Bebidas', 'Suco de maracujá', 'Copo de 300 ml', 6, ''],
  ['Bebidas', 'Refrigerante lata', 'Lata de 350 ml', 5.5, ''],
  ['Bebidas', 'Chá gelado', 'Garrafa de 300 ml', 5, ''],
  ['Bebidas', 'Achocolatado', 'Caixinha de 200 ml', 4.5, 'Lactose,Soja'],
  ['Bebidas', 'Iogurte', 'Pote de 170 g', 5, 'Lactose'],
  ['Bebidas', 'Vitamina de banana', 'Copo de 300 ml', 7.5, 'Lactose'],
  ['Bebidas', 'Leite', 'Copo de 200 ml', 3.5, 'Lactose'],
  ['Bebidas', 'Café com leite', 'Copo de 200 ml', 4, 'Lactose'],
];
export const catalogoInicial = dados.map(([categoria, nome, descricao, preco, alergia], index) => ({ id: index + 1, categoria, nome, descricao, preco, alergenos: alergia ? alergia.split(',') : [], estoque: 20, ativo: true, disponivel: true }));
export function dataLocal(date = new Date()) { return date.getFullYear() + '-' + String(date.getMonth() + 1).padStart(2, '0') + '-' + String(date.getDate()).padStart(2, '0'); }
export function intervaloAberto(key, date = dataLocal(), now = new Date()) {
  const i = intervalos[key]; if (!i || date !== dataLocal(now)) return false;
  const [h, m] = i.fechamento.split(':').map(Number); return now.getHours() * 60 + now.getMinutes() < h * 60 + m;
}
export function minutosRestantes(key, now = new Date()) {
  const i = intervalos[key]; if (!i) return 0;
  const [h, m] = i.fechamento.split(':').map(Number);
  return Math.max(0, h * 60 + m - now.getHours() * 60 - now.getMinutes());
}

export const dinheiro = (n) => new Intl.NumberFormat('pt-BR',{style:'currency',currency:'BRL'}).format(Number(n)||0);
