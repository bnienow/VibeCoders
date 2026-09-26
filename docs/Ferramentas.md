# Ferramentas complementares — Hackathon (16h)

## Dados de teste
- **Mockaroo** (mockaroo.com) — gera dados fake realistas em CSV/JSON/SQL pra popular o MySQL rápido, sem digitar INSERT na mão.

## Diagramas / arquitetura (pro pitch)
- **Excalidraw** (excalidraw.com) — desenho rápido de diagramas à mão livre.
- **Napkin.ai** (napkin.ai) — transforma texto em diagrama visual automaticamente.

## Slides / apresentação
- **Gamma.app** (gamma.app) — gera apresentação inteira a partir de texto descrevendo o conteúdo.

## Pesquisa de domínio
- **Perplexity** (perplexity.ai) — pesquisa rápida com fontes citadas, útil se o problema exigir contexto de domínio desconhecido pelo time.

## Assistência de código pontual (complementar ao Cowork)
- **Claude.ai / ChatGPT** — tirar dúvida rápida sem interromper o fluxo do Cowork (ex: erro de CORS, query LINQ específica).

## Automação / notificações
- **n8n** — fluxos visuais de automação (email, WhatsApp, Slack) disparados via webhook chamado pela API .NET.
- **SendGrid** ou **Resend** — serviços de envio de email (API key, sem precisar configurar SMTP manualmente tipo Gmail).

## Autenticação pronta
- **Clerk** (clerk.com) — login/registro pronto, componentes React inclusos, mantém seu MySQL como está.
- **Supabase Auth** (supabase.com) — auth pronta, mas exige Postgres (não MySQL).

## Geração de UI rápida (se front não for foco)
- **v0.dev** — gera componentes React a partir de descrição/prompt.
- **bolt.new** — gera projeto front completo a partir de prompt.

---

### Prioridade sugerida (maior retorno pro tempo investido)
1. Mockaroo — dados de demo realistas
2. Gamma — slides do pitch
3. n8n + SendGrid/Resend — se o problema envolver notificação
4. Clerk — só se autenticação for realmente necessária no fluxo