import { Navigate, Route, Routes } from 'react-router-dom';
import Estrutura from './components/Estrutura';
import Tema from './components/Tema';
import CadastroAdulto from './pages/CadastroAdulto';
import CadastroAluno from './pages/CadastroAluno';
import Cardapio from './pages/Cardapio';
import Catalogo from './pages/Catalogo';
import Historico from './pages/Historico';
import Home from './pages/Home';
import Login from './pages/Login';
import Perfil from './pages/Perfil';
import RevisaoPedido from './pages/RevisaoPedido';

export default function App() {
  return <>
    <Tema />
    <Routes>
      {/* Públicas */}
      <Route path="/login" element={<Login />} />
      <Route path="/cadastro-adulto" element={<CadastroAdulto />} />
      <Route path="/cadastro-aluno" element={<CadastroAluno />} />

      {/* Área logada: a Estrutura exige login e desenha cabeçalho e rodapé em volta da página */}
      <Route element={<Estrutura />}>
        <Route path="/home" element={<Home />} />
        <Route path="/cardapio" element={<Cardapio />} />
        <Route path="/revisao" element={<RevisaoPedido />} />
        <Route path="/historico" element={<Historico />} />
        <Route path="/catalogo" element={<Catalogo />} />
        <Route path="/perfil" element={<Perfil />} />
      </Route>

      {/* Qualquer outro endereço vai para a home (que manda para o login se não houver sessão) */}
      <Route path="*" element={<Navigate to="/home" replace />} />
    </Routes>
  </>;
}
