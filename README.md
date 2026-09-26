# VibeCoders — Cantina Escolar

Projeto do Hackathon de Programação 2026 (Nexus / Instituto Ivoti): sistema para a cantina deixar o caderninho —
pedido antecipado pelo aluno, venda no balcão, painel de retirada, conta com crédito e fiado, extrato e fechamento
mensal para o responsável.

- Especificação completa: [`docs/especificacao-cantina.md`](docs/especificacao-cantina.md)
- Uso de IA: [`IA.md`](IA.md)

## Tecnologias

### Backend
| Tecnologia | Versão | Para quê |
|---|---|---|
| .NET / ASP.NET Core | 8.0 | API REST com controllers |
| Entity Framework Core | 8.0.2 | ORM (mapeamento das entidades, migrations, consultas LINQ) |
| Pomelo.EntityFrameworkCore.MySql | 8.0.2 | Provedor do EF Core para MySQL |
| Microsoft.EntityFrameworkCore.Design | 8.0.2 | Ferramentas de migration (`dotnet ef`) |
| MySQL | 8.0 | Banco de dados |
| QuestPDF | 2024.10.3 | PDF do extrato |
| ClosedXML | 0.104.1 | Planilha Excel dos relatórios |
| Swashbuckle.AspNetCore | 6.6.2 | Swagger (documentação e teste das rotas) |

Recursos do próprio ASP.NET, sem pacote extra: autenticação por **cookie** e **`PasswordHasher`** (hash de senha PBKDF2).

### Frontend
| Tecnologia | Versão | Para quê |
|---|---|---|
| React | 19 | Interface |
| React Router DOM | 7 | Rotas das páginas |
| Vite | 8 | Servidor de desenvolvimento, build e proxy `/api` → backend |
| ESLint (+ plugins react-hooks e react-refresh) | 10 | Verificação do código |

Sem biblioteca de UI: o estilo fica em um único arquivo (`src/components/Tema.jsx`). Chamadas à API com `fetch`.

## Estrutura

```
VibeCoders/
├── backend/                         API ASP.NET Core
│   ├── Controllers/                 rotas: Auth, Itens, Intervalos, Pedidos, Painel, Alunos,
│   │                                Extratos, Fechamentos, Pagamentos, Relatorios
│   ├── Services/                    regras de negócio (pedido, conta, fechamento, alergia)
│   ├── Models/                      entidades do banco + Enums/
│   ├── DTOs/                        formatos de entrada e saída da API (nunca expõe entidade)
│   ├── Data/                        AppDbContext, SeedData (massa de teste) e Migrations/
│   ├── Mappings/                    conversão Pedido → PedidoDto
│   ├── Extensions/                  usuário logado (cookie) e regra de acesso a aluno
│   ├── Reports/                     exportação PDF e Excel
│   ├── Program.cs                   configuração: banco, cookie, serviços, tratamento de erros
│   └── appsettings*.json            configuração (a connection string fica no Development, fora do Git)
│
├── frontend/                        React + Vite
│   ├── src/
│   │   ├── pages/                   telas (aluno, responsável, cantina, login e cadastro)
│   │   ├── components/              layout (Estrutura), tema, cartões, campos, faixa offline
│   │   ├── services/                uma função por rota da API (api.js é o único fetch)
│   │   ├── hooks/                   useApi, useAuth, useCarrinho, useConexao, useRelogio
│   │   ├── contexts/                sessão e carrinho compartilhados
│   │   ├── App.jsx                  rotas por perfil
│   │   └── rotas.js                 menu e página inicial de cada perfil
│   └── vite.config.js               proxy /api → http://localhost:5241
│
├── docs/                            especificação e documentos de apoio
├── IA.md                            registro do uso de IA
└── VibeCoders.sln
```

## Como rodar

Pré-requisitos: .NET 8 SDK, Node.js, MySQL e a ferramenta `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

1. **Configurar o banco**: copie `backend/appsettings.Example.json` para `backend/appsettings.Development.json` e preencha a connection string.
2. **Backend** (cria as tabelas e, com o banco vazio, popula a massa de teste ao subir):
   ```bash
   cd backend
   dotnet ef database update
   dotnet run                # http://localhost:5241 (Swagger em /swagger)
   ```
3. **Frontend**, em outro terminal:
   ```bash
   cd frontend
   npm install
   npm run dev               # http://localhost:5173
   ```

## Contas de teste (dados fictícios)

| Perfil | E-mail | Senha |
|---|---|---|
| Admin (cantina) | `admin@cantina.test` | `admin123` |
| Responsável | `carla.andrade@email.test` | `senha123` |
| Aluno | `lucas.andrade@aluno.cantina.test` | `senha123` |
| Aluno perto do teto de fiado | `pedro.kunz@aluno.cantina.test` | `senha123` |
| Aluna com restrição de lactose | `gabriela.schmitt@aluno.cantina.test` | `senha123` |
| Aluno com limite diário | `rafael.schmitt@aluno.cantina.test` | `senha123` |

## Funcionalidades

- **Aluno**: cardápio por dia e intervalo, pedido antecipado com código de retirada, alterar/cancelar enquanto a janela está aberta, histórico, extrato (PDF) e perfil
- **Responsável**: visão da família, crédito, limite diário, restrições alimentares, extrato dos filhos, fechamento mensal, métodos de pagamento (simulados) e cadastro de filho
- **Cantina (Admin)**: painel do intervalo (preparo e entrega), balcão (PDV) com lançamento na conta ou à vista, gestão de itens e cardápio do dia, relatórios (Excel) e geração do fechamento
- **Regras**: teto de fiado de R$ 250 e limite diário (só aluno), pedidos fecham 15 min antes do intervalo, preço congelado no pedido, alerta de alergia
- **Modo offline do balcão**: sem conexão com a API, as vendas ficam guardadas no navegador e são enviadas quando a conexão volta

  ## Figma Linl
  [https://www.figma.com/design/u6HNQZYWATlLWYRcxZtNRr/Cantina---Vibe-Codes?node-id=0-1&t=PIVU6MPRJEWMX9XZ-1](url)
