// Página inicial e menu de cada perfil (usados pelo login e pelo cabeçalho)
export const PAGINA_INICIAL = { Aluno: '/home', Adulto: '/responsavel', Admin: '/cantina' };

export const MENU = {
  Aluno: [['Home', '/home'], ['Cardápio', '/cardapio'], ['Meus pedidos', '/historico'], ['Extrato', '/extrato'], ['Perfil', '/perfil']],
  Adulto: [['Família', '/responsavel'], ['Gestão dos filhos', '/responsavel/filhos'], ['Extrato', '/responsavel/extrato'], ['Fechamento', '/responsavel/fechamento'], ['Pagamentos', '/responsavel/pagamentos'], ['Cadastrar filho', '/responsavel/cadastrar-filho']],
  Admin: [['Painel', '/cantina'], ['Balcão', '/cantina/pdv'], ['Itens', '/cantina/itens'], ['Relatórios', '/cantina/relatorios']],
};
