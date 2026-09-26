# Uso de IA no projeto

Ferramentas usadas: **Claude Code** (Anthropic) e **OpenAI Codex** (primeira versão das telas).

## Backend (ASP.NET Core 8)

### Gerado por IA

| Parte | Arquivos |
|---|---|
| Services (regras de negócio) | `Services/PedidoService.cs`, `ContaService.cs`, `FechamentoService.cs`, `Alergia.cs`, `RegraException.cs` |
| Reports (exportação) | `Reports/PdfExportService.cs` (QuestPDF), `Reports/ExcelExportService.cs` (ClosedXML) |
| Maioria dos DTOs | `DTOs/Pedidos`, `DTOs/Painel`, `DTOs/Extratos`, `DTOs/Fechamentos`, `DTOs/Pagamentos`, `DTOs/Relatorios`, `DTOs/Itens` |
| Controllers mais complexos | `PedidosController`, `PainelController`, `AlunosController`, `ExtratosController`, `FechamentosController`, `PagamentosController`, `RelatoriosController`, `ItensController` |
| Apoio | `Mappings/PedidoMapper.cs`, `Extensions/*`, `Data/SeedData.cs` (massa de teste) |

## Frontend (React + Vite)

### Gerado por IA

| Parte | Arquivos |
|---|---|
| Componentes | `src/components/*` |
| Hooks | `src/hooks/*` |
| Páginas | `src/pages/*` |
| Demais | `src/services/*` (chamadas à API), `src/contexts/*`, `src/App.jsx`, `src/rotas.js`, `src/main.jsx` |

