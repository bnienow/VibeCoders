import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App.jsx';
import AuthProvider from './components/AuthProvider';
import CarrinhoProvider from './components/CarrinhoProvider';

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <CarrinhoProvider>
          <App />
        </CarrinhoProvider>
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
);
