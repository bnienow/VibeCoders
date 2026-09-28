# Uso de IA no projeto

Ferramentas usadas: **Claude Code** (Anthropic) e **OpenAI Codex** (primeira versão das telas).

## Backend (ASP.NET Core 8)

### Gerado por IA

| Parte | Arquivos |
|---|---|
| Services (regras de negócio) | `Data/Services/*`, `Domain/Interfaces/Services/*`, `Domain/Helpers/Alergia.cs`, `Domain/Exceptions/*` |
| Repositories (acesso ao banco) | `Data/Repositories/*`, `Domain/Interfaces/Repositories/*` |
| Reports (exportação) | `Api/Reports/PdfExportService.cs` (QuestPDF), `Api/Reports/ExcelExportService.cs` (ClosedXML) |
| Maioria dos DTOs | `Domain/DTOs/Pedidos`, `Domain/DTOs/Painel`, `Domain/DTOs/Extratos`, `Domain/DTOs/Fechamentos`, `Domain/DTOs/Pagamentos`, `Domain/DTOs/Relatorios`, `Domain/DTOs/Itens` |
| Controllers mais complexos | `PedidosController`, `PainelController`, `AlunosController`, `ExtratosController`, `FechamentosController`, `PagamentosController`, `RelatoriosController`, `ItensController` |
| Apoio | `Domain/Mappings/PedidoMapper.cs`, `Api/Extensions/*`, `Data/SeedData.cs` (massa de teste) |

## Frontend (React + Vite)

### Gerado por IA

| Parte | Arquivos |
|---|---|
| Componentes | `src/components/*` |
| Hooks | `src/hooks/*` |
| Páginas | `src/pages/*` |
| Demais | `src/services/*` (chamadas à API), `src/contexts/*`, `src/App.jsx`, `src/rotas.js`, `src/main.jsx` |

