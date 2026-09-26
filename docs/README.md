# VibeCoders — Cantina Escolar

Projeto do Hackathon de Programação 2026 — Nexus / Instituto Ivoti.

A [especificação técnica da cantina](docs/especificacao-cantina.md) descreve o modelo proposto. Entidades e rotas descritas nela podem ainda não estar implementadas no código.

## Registro de alterações feitas com IA

Toda alteração produzida com auxílio de IA neste repositório deve ser registrada aqui com data, ferramenta, arquivos afetados e resumo. Esse registro não substitui a declaração exigida pela comissão do evento.

| Data (Brasília) | Ferramenta | Arquivos | Alteração |
|---|---|---|---|
| 25/09/2026 | OpenAI Codex (ChatGPT Work) | `docs/especificacao-cantina.md`, `README.md` | Ajuste da especificação com somente duas novas entidades: `Conta` centraliza os saldos de alunos e adultos e os movimentos financeiros; `DispCardapio` controla a oferta de itens por data/intervalo. Regras, consultas, estrutura proposta e massa de teste foram atualizadas para essas duas entidades. Este README registra a alteração feita com IA. |
| 25/09/2026 | Claude Code (Claude Opus 5.5) | `docs/especificacao-cantina.md` | Autenticação por cookie nativo do ASP.NET Core no lugar de JWT e proxy do Vite no lugar de CORS; limite diário e teto de fiado valem só para aluno; aluno obrigatoriamente vinculado a um adulto; matrícula removida (aluno identificado pelo e-mail institucional); todo pedido ligado a um usuário (venda de balcão nasce `Entregue`, sem fila); `Pedido.IntervaloId` opcional para balcão fora de intervalo; `Aluno.DataNascimento`; conta `Admin` única criada pelo seed; enums `TipoMetodoPagamento` e `StatusFechamento`. Mesclado com a alteração de `Conta`/`DispCardapio`. |
