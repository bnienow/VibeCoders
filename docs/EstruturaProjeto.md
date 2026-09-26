# Estrutura do Projeto — Hackathon

## Backend (ASP.NET Core)

```
/backend
├── Controllers/
│   ├── ProdutosController.cs
│   ├── PedidosController.cs
│   └── UsuariosController.cs
│
├── Models/                       # Entidades (mapeiam as tabelas)
│   ├── Produto.cs
│   ├── Pedido.cs
│   └── Usuario.cs
│
├── DTOs/                         # O que trafega na API (nunca expõe Model direto)
│   ├── ProdutoDto.cs
│   ├── CreateProdutoDto.cs
│   └── PedidoDto.cs
│
├── Data/
│   ├── AppDbContext.cs           # DbContext principal
│   └── Migrations/               # gerado automaticamente pelo EF Core
│
├── Mappings/
│   └── AutoMapperProfile.cs      # perfis de mapeamento Model <-> DTO
│
├── Services/
│   ├── NotificacaoService.cs     # SendGrid (email) + Twilio (WhatsApp)
│   └── PedidoService.cs          # regra de negócio, se existir (opcional)
│
├── Reports/                      # geração de arquivo
│   ├── ExcelExportService.cs     # ClosedXML
│   └── PdfExportService.cs       # QuestPDF
│
├── appsettings.json              # NÃO vai pro Git (connection string, API keys reais)
├── appsettings.Example.json      # vai pro Git (estrutura sem segredo)
├── Program.cs                    # DbContext, CORS, AutoMapper, Services, Swagger
├── Backend.csproj
└── .gitignore                    # (ou usa o da raiz do monorepo)
```

**Fluxo de comunicação:**

```
Controller → Service (opcional) → AppDbContext (EF Core) → MySQL
Controller → NotificacaoService → SendGrid / Twilio (APIs externas)
```

---

## Frontend (React + Vite)

```
/frontend
├── public/
│   └── favicon.svg
│
├── src/
│   ├── assets/                   # imagens, ícones
│   │
│   ├── components/                # componentes reutilizáveis
│   │   ├── ProdutoCard.jsx
│   │   ├── Header.jsx
│   │   └── Loading.jsx
│   │
│   ├── pages/                     # telas/rotas principais
│   │   ├── Home.jsx
│   │   ├── Produtos.jsx
│   │   └── Pedidos.jsx
│   │
│   ├── services/                  # camada que fala com a API
│   │   ├── api.js                 # config base do fetch/axios
│   │   └── produtosService.js     # getProdutos, createProduto...
│   │
│   ├── hooks/                     # hooks customizados (opcional)
│   │   └── useProdutos.js
│   │
│   ├── App.jsx                    # rotas (React Router, se usar)
│   ├── main.jsx                   # entry point
│   └── index.css
│
├── .env                            # NÃO vai pro Git (URL da API)
├── .env.example                    # vai pro Git
├── index.html
├── package.json
├── vite.config.js
└── .gitignore                      # (ou usa o da raiz do monorepo)
```

---

## Raiz do monorepo

```
/hackathon-projeto
├── backend/
├── frontend/
├── .gitignore
├── git-comandos.md
├── ferramentas-hackathon.md
└── README.md                       # como rodar os dois lados
```

---

## Regras de decisão rápida (pra não perder tempo decidindo de novo)

- **Repository:** não usa — `AppDbContext` já cumpre esse papel.
- **Service:** só cria quando há chamada a API externa (SendGrid/Twilio) ou regra de negócio real (cálculo, validação). CRUD simples fica direto no Controller.
- **DTOs:** sempre — nunca retorna `Model` do EF Core direto na API.
- **AutoMapper:** usa pra converter Model <-> DTO, evita mapeamento manual repetitivo.
