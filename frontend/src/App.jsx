import { Navigate, Route, Routes } from 'react-router-dom';
import Estrutura from './components/Estrutura';
import Tema from './components/Tema';
import CadastroAdulto from './pages/CadastroAdulto';
import CadastroAluno from './pages/CadastroAluno';
import Cardapio from './pages/Cardapio';
import ExtratoFilho from './pages/ExtratoFilho';
import FechamentoMensal from './pages/FechamentoMensal';
import GestaoFilhos from './pages/GestaoFilhos';
import GestaoItens from './pages/GestaoItens';
import Historico from './pages/Historico';
import Home from './pages/Home';
import Login from './pages/Login';
import MetodosPagamento from './pages/MetodosPagamento';
import MeuExtrato from './pages/MeuExtrato';
import PainelIntervalo from './pages/PainelIntervalo';
import PainelResponsavel from './pages/PainelResponsavel';
import PdvBalcao from './pages/PdvBalcao';
import Perfil from './pages/Perfil';
import Relatorios from './pages/Relatorios';
import RevisaoPedido from './pages/RevisaoPedido';

export default function App() {
  return <>
    <Tema />
    <Routes>
      {/* Públicas */}
      <Route path="/login" element={<Login />} />
      <Route path="/cadastro-adulto" element={<CadastroAdulto />} />

      {/* Aluno */}
      <Route element={<Estrutura permissoes={['Aluno']} />}>
        <Route path="/home" element={<Home />} />
        <Route path="/cardapio" element={<Cardapio />} />
        <Route path="/revisao" element={<RevisaoPedido />} />
        <Route path="/historico" element={<Historico />} />
        <Route path="/extrato" element={<MeuExtrato />} />
        <Route path="/perfil" element={<Perfil />} />
      </Route>

      {/* Responsável */}
      <Route element={<Estrutura permissoes={['Adulto']} />}>
        <Route path="/responsavel" element={<PainelResponsavel />} />
        <Route path="/responsavel/filhos" element={<GestaoFilhos />} />
        <Route path="/responsavel/extrato" element={<ExtratoFilho />} />
        <Route path="/responsavel/fechamento" element={<FechamentoMensal />} />
        <Route path="/responsavel/pagamentos" element={<MetodosPagamento />} />
        <Route path="/responsavel/cadastrar-filho" element={<CadastroAluno />} />
      </Route>

      {/* Cantina (Admin) */}
      <Route element={<Estrutura permissoes={['Admin']} />}>
        <Route path="/cantina" element={<PainelIntervalo />} />
        <Route path="/cantina/pdv" element={<PdvBalcao />} />
        <Route path="/cantina/itens" element={<GestaoItens />} />
        <Route path="/cantina/relatorios" element={<Relatorios />} />
      </Route>

      {/* Qualquer outro endereço: /home, que redireciona para o login ou para a página inicial do perfil */}
      <Route path="*" element={<Navigate to="/home" replace />} />
    </Routes>
  </>;
}
