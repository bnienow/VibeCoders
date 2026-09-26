# Cantina Escolar — Especificação Técnica

**Hackathon de Programação 2026 — Nexus / Instituto Ivoti**
Desafio: "A cantina ainda anota no caderninho"

---

## 1. Contexto e números

| Dado | Valor |
|---|---|
| Alunos na escola | 840 |
| Intervalos por dia | 2 (09:00 e 15:30) |
| Minutos por intervalo | 20 |
| Atendimentos por intervalo | ~230 |
| Itens no cardápio | 38 |
| Pessoas atendendo | 3 |

**Implicação de projeto:** ~5 segundos por atendimento no balcão. A tela do balcão é a parte mais crítica do sistema — deve funcionar com poucos toques, sem navegação entre páginas, com busca rápida por nome/código.

---

## 2. Atores

| Ator | Quem é | O que faz |
|---|---|---|
| **Aluno** | 11 a 18 anos | Vê cardápio, faz pedido antecipado, compra no balcão, vê o próprio saldo e pedidos |
| **Adulto (responsável)** | Pai, mãe ou tutor | Coloca crédito, define limite diário, vê extrato do filho, define método de pagamento, paga fechamento mensal |
| **Cantina** | 3 atendentes | Vê pedidos do intervalo, entrega, registra venda no balcão, gerencia itens/estoque, vê relatórios |

---

## 3. Requisitos — rastreabilidade

### Obrigatórios

| ID | Requisito | Onde é atendido |
|---|---|---|
| R1 | Cadastro de alunos e responsáveis, com vínculo | `UsuariosController`, `AlunosController`, `AdultosController` — telas de Cadastro e Gestão de Filhos |
| R2 | Cardápio do dia com itens, preços e disponibilidade | `ItensController.GetCardapio()` — tela Cardápio |
| R3 | Pedido antecipado (monta pedido + escolhe intervalo) | `PedidosController.Create()` com `TipoVenda = Antecipado` |
| R4 | Retirada identificada por código, senha ou nome | `Pedido.CodigoRetirada` + busca no Painel da Cantina |
| R5 | Venda direta no balcão, poucos toques, lançamento na conta ou pagamento na hora | `PedidosController.CreateBalcao()` — tela PDV Balcão |
| R6 | Lançamento na conta do aluno a cada compra | `Movimento` + atualização de `Saldo` na mesma transação |
| R7 | Extrato e fechamento mensal, com histórico de consumo | `ExtratosController`, `FechamentosController` — tela Extrato |
| R8 | Painel da cantina com pedidos do próximo intervalo | `PainelController.GetIntervaloAtual()` — tela Painel |

### Desejáveis

| ID | Requisito | Onde é atendido |
|---|---|---|
| D1 | Pagamento online (Pix ou cartão), simulado | `PagamentosController` — serviço `GatewaySimuladoService` |
| D2 | Limite de gasto diário definido pelo responsável | `Aluno.LimiteDiario` + validação em `ValidacaoPedidoService` |
| D3 | Alergias e restrições alimentares sinalizadas no pedido | `Item.Alergenos` + `Aluno.RestricoesAlimentares` + alerta no pedido e no painel |
| D4 | Aviso de saldo baixo para o responsável | `NotificacaoService` (e-mail/WhatsApp) + badge na tela |
| D5 | Relatório de vendas por período e itens mais vendidos | `RelatoriosController` + export Excel/PDF |
| D6 | Modo de contingência para quando a internet cair | Cache local do painel/PDV + fila de sincronização |

---

## 4. Regras de negócio

| Regra | Implementação |
|---|---|
| **O balcão nunca fecha** | Pedido antecipado é caminho a mais, não o único. `TipoVenda = Balcao` sempre disponível, independente de horário |
| **Fiado tem teto** | Conta pode ficar negativa até **R$ 250,00**. Passou disso, só à vista. Validado em `ValidacaoPedidoService` |
| **Pedido fecha antes** | Pedidos antecipados fecham **15 minutos antes** do intervalo começar. Depois disso, cozinha já está montando |
| **Um pedido por intervalo** | Um `Pedido` por aluno por intervalo. Incluir mais itens = alterar o pedido existente enquanto estiver `Aberto` |
| **Cancelar só antes do fechamento** | Depois que fecha, sem cancelamento nem estorno. Pedidos já feitos são honrados |
| **Item esgotado some** | `Estoque = 0` → item sai do cardápio, mas pedidos já feitos continuam válidos. Estoque é decrementado na **confirmação**, não na entrega |
| **O limite é do responsável** | Aluno não altera o próprio limite de gasto nem o próprio saldo. Bloqueio por permissão |
| **Fechamento no dia 1º** | Cada responsável recebe o consolidado do mês anterior, item a item |

### Restrições técnicas (do briefing)

- Pagamento online **pode ser simulado** — não integrar gateway real
- **Nenhum dado real** de aluno, responsável ou funcionário — massa de teste inventada
- Solução precisa **rodar na máquina** durante a apresentação
- Linguagem, arquitetura e plataforma livres

### O que o cliente NÃO pediu (fora de escopo)

- Emissão de nota fiscal / integração contábil
- Aplicativo publicado nas lojas
- Integração com sistema acadêmico
- Controle de estoque do fornecedor e compras
- Catraca, biometria ou carteirinha física

---

## 5. Stack

### Backend
- **C# / ASP.NET Core 8** (Controllers convencionais)
- **Entity Framework Core 8** + **Pomelo.EntityFrameworkCore.MySql 8.0.2**
- **MySQL** local
- **AutoMapper 12.0.1**
- **ClosedXML 0.104.1** (export Excel)
- **QuestPDF 2024.10.3** (export PDF)
- **Swashbuckle.AspNetCore 6.6.2** (Swagger)

### Frontend
- **React + Vite**
- `fetch` para consumo da API
- CORS liberado para `http://localhost:5173`

### Integrações externas (opcionais, D4)
- **SendGrid** (e-mail)
- **Twilio** (WhatsApp)

---

## 6. Modelo de dados

### Usuario
Entidade genérica de autenticação. Aluno e Adulto especializam.

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| Nome | string | |
| Email | string | único |
| SenhaHash | string | |
| Permissao | enum | `Aluno` \| `Adulto` \| `Cantina` |
| Ativo | bool | |
| CriadoEm | DateTime | |

### Aluno

| Campo | Tipo | Observação |
|---|---|---|
| UsuarioId | int | PK / FK → Usuario |
| AdultoId | int | FK → Adulto (N alunos → 1 adulto) |
| Matricula | string | usada na busca do balcão |
| Turma | string | |
| LimiteDiario | decimal? | definido pelo responsável (D2). Null = sem limite |
| Saldo | decimal | positivo = crédito; negativo = fiado (mínimo -250) |
| RestricoesAlimentares | string | lista de alérgenos, separada por vírgula (D3) |

### Adulto

| Campo | Tipo | Observação |
|---|---|---|
| UsuarioId | int | PK / FK → Usuario |
| Cpf | string | |
| Telefone | string | usado no WhatsApp (D4) |
| Saldo | decimal | responsável também pode comprar |
| MetodoPagamentoPadraoId | int? | FK → MetodoPagamento |

### Item

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| Nome | string | |
| **Descricao** | string | descrição do item exibida no cardápio |
| PrecoUnitario | decimal | |
| Estoque | int | 0 = esgotado, some do cardápio |
| Categoria | enum | `Salgado` \| `Doce` \| `Bebida` \| `Combo` |
| Alergenos | string | `Gluten,Lactose,Amendoim...` (D3) |
| Ativo | bool | desativa sem apagar histórico |

### Pedido

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| UsuarioId | int | FK → Usuario (aluno **ou** adulto) |
| Data | DateOnly | |
| IntervaloId | int | FK → Intervalo |
| Status | enum | `Aberto` \| `Confirmado` \| `Entregue` \| `Cancelado` |
| TipoVenda | enum | `Antecipado` \| `Balcao` |
| FormaPagamento | enum | `Conta` \| `AVista` \| `Online` |
| Total | decimal | soma dos `ItemPedido.Subtotal` |
| CodigoRetirada | string | código curto (ex: `A7K2`) — R4 |
| TemAlertaAlergia | bool | calculado na criação (D3) |
| CriadoEm / EntregueEm / CanceladoEm | DateTime? | |

### ItemPedido

| Campo | Tipo | Observação |
|---|---|---|
| PedidoId | int | PK composta / FK |
| ItemId | int | PK composta / FK |
| Quantidade | int | |
| **PrecoUnitario** | decimal | **congelado** no momento do pedido |
| Subtotal | decimal | `Quantidade * PrecoUnitario` |

> **Por que congelar o preço:** se a cantina mudar o preço amanhã, o extrato do mês passado não pode mudar junto. Sem isso, a cena 4 e o fechamento do dia 1º ficam errados.

### Movimento
Conta corrente do usuário — alimenta extrato (R7) e fechamento.

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| UsuarioId | int | FK → Usuario |
| Tipo | enum | `Compra` \| `Credito` \| `Pagamento` \| `Estorno` |
| Valor | decimal | negativo em Compra, positivo em Crédito/Pagamento |
| Data | DateTime | |
| PedidoId | int? | FK → Pedido (null em crédito/pagamento) |
| Descricao | string | |
| SaldoApos | decimal | snapshot, facilita o extrato |

### Intervalo

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| Nome | string | `Manhã` \| `Tarde` |
| HoraInicio | TimeOnly | 09:00 / 15:30 |
| HoraFim | TimeOnly | 09:20 / 15:50 |
| MinutosAntecedencia | int | 15 — fecha pedidos |

### MetodoPagamento (D1)

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| AdultoId | int | FK → Adulto |
| Tipo | enum | `Pix` \| `Cartao` |
| Apelido | string | "Cartão Nubank" |
| UltimosDigitos | string | dados fictícios |
| Ativo | bool | |

### Fechamento (R7)

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| AdultoId | int | FK → Adulto |
| MesReferencia | DateOnly | primeiro dia do mês fechado |
| ValorTotal | decimal | |
| Status | enum | `Aberto` \| `Pago` |
| GeradoEm / PagoEm | DateTime? | |

---

## 7. Estrutura do backend

```
/backend
├── Controllers/
│   ├── AuthController.cs
│   ├── UsuariosController.cs
│   ├── AlunosController.cs
│   ├── AdultosController.cs
│   ├── ItensController.cs
│   ├── PedidosController.cs
│   ├── PainelController.cs
│   ├── ExtratosController.cs
│   ├── FechamentosController.cs
│   ├── PagamentosController.cs
│   └── RelatoriosController.cs
│
├── Models/
│   ├── Usuario.cs
│   ├── Aluno.cs
│   ├── Adulto.cs
│   ├── Item.cs
│   ├── Pedido.cs
│   ├── ItemPedido.cs
│   ├── Movimento.cs
│   ├── Intervalo.cs
│   ├── MetodoPagamento.cs
│   ├── Fechamento.cs
│   └── Enums/
│       ├── Permissao.cs
│       ├── StatusPedido.cs
│       ├── TipoVenda.cs
│       ├── FormaPagamento.cs
│       ├── TipoMovimento.cs
│       └── CategoriaItem.cs
│
├── DTOs/
│   ├── Auth/           (LoginDto, LoginResponseDto, RegistroDto)
│   ├── Alunos/         (AlunoDto, CreateAlunoDto, UpdateLimiteDto)
│   ├── Itens/          (ItemDto, CardapioItemDto, CreateItemDto, UpdateEstoqueDto)
│   ├── Pedidos/        (PedidoDto, CreatePedidoDto, CreatePedidoBalcaoDto, ItemPedidoDto)
│   ├── Painel/         (PedidoPainelDto)
│   ├── Extratos/       (ExtratoDto, MovimentoDto)
│   └── Relatorios/     (VendasPeriodoDto, ItemMaisVendidoDto)
│
├── Data/
│   ├── AppDbContext.cs
│   ├── SeedData.cs             ← massa de teste fictícia
│   └── Migrations/
│
├── Mappings/
│   └── AutoMapperProfile.cs
│
├── Services/
│   ├── ValidacaoPedidoService.cs    ← teto R$250 + limite diário + estoque + janela
│   ├── ContaService.cs              ← Movimento + atualização de saldo (transacional)
│   ├── PedidoService.cs             ← criação/alteração/cancelamento
│   ├── FechamentoService.cs         ← consolidado do dia 1º
│   ├── GatewaySimuladoService.cs    ← D1
│   ├── NotificacaoService.cs        ← D4 (SendGrid + Twilio)
│   └── AlergiaService.cs            ← D3
│
├── Reports/
│   ├── ExcelExportService.cs        ← ClosedXML
│   └── PdfExportService.cs          ← QuestPDF
│
├── appsettings.json                 (não vai pro Git)
├── appsettings.Example.json         (vai pro Git)
├── Program.cs
└── Backend.csproj
```

### Fluxo de chamadas

```
Controller → Service → AppDbContext (EF Core) → MySQL
Controller → NotificacaoService → SendGrid / Twilio
Controller → Excel/PdfExportService → arquivo de download
```

Sem camada de Repository — o `DbContext` já cumpre esse papel. CRUD simples fica direto no Controller; Service só onde há regra de negócio real ou chamada externa.

---

## 8. Rotas da API

### Auth
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/registro` | Cria conta (Aluno ou Adulto) — R1 |
| POST | `/api/auth/login` | Retorna JWT + permissão |
| GET | `/api/auth/me` | Dados do usuário logado |

### Alunos
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/alunos` | Lista (filtro `?busca=` por nome/matrícula — usado no balcão) |
| GET | `/api/alunos/{id}` | Detalhe com saldo e restrições |
| POST | `/api/alunos` | Cadastra aluno vinculado a um adulto — R1 |
| GET | `/api/adultos/{id}/filhos` | Filhos do responsável |
| PUT | `/api/alunos/{id}/limite` | Define limite diário — D2 (só Adulto) |
| PUT | `/api/alunos/{id}/restricoes` | Define alergias — D3 |
| POST | `/api/alunos/{id}/credito` | Responsável adiciona crédito |

### Itens / Cardápio
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/itens/cardapio` | Cardápio do dia: ativos com `Estoque > 0` — R2 |
| GET | `/api/itens` | Todos (gestão da cantina) |
| POST | `/api/itens` | Cria item |
| PUT | `/api/itens/{id}` | Edita nome, descrição, preço, alérgenos |
| PUT | `/api/itens/{id}/estoque` | Ajusta estoque |
| DELETE | `/api/itens/{id}` | Desativa (soft delete) |

### Pedidos
| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/pedidos` | Pedido antecipado — R3. Valida janela, limite, teto, estoque |
| POST | `/api/pedidos/balcao` | Venda direta — R5. Nasce `Entregue` |
| GET | `/api/pedidos/meus` | Pedidos do usuário logado |
| GET | `/api/pedidos/{id}` | Detalhe |
| PUT | `/api/pedidos/{id}` | Altera itens (só se `Aberto`) |
| DELETE | `/api/pedidos/{id}` | Cancela (só antes do fechamento) |
| POST | `/api/pedidos/{id}/entregar` | Marca entregue — R4 |

### Painel da cantina (R8)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/painel/intervalo-atual` | Pedidos do próximo intervalo, agrupados |
| GET | `/api/painel/preparo` | Consolidado por item: quanto precisa ser preparado |
| GET | `/api/painel/buscar?termo=` | Busca por código, nome ou matrícula — R4 |

### Extratos e fechamento (R7)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/extratos/aluno/{id}?mes=` | Extrato item a item |
| GET | `/api/extratos/aluno/{id}/pdf` | Export PDF (QuestPDF) |
| GET | `/api/fechamentos/adulto/{id}` | Fechamentos do responsável |
| POST | `/api/fechamentos/gerar` | Gera consolidado do mês anterior (dia 1º) |
| POST | `/api/fechamentos/{id}/pagar` | Paga fechamento — D1 |

### Pagamentos (D1 — simulado)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/pagamentos/metodos` | Métodos do adulto |
| POST | `/api/pagamentos/metodos` | Cadastra método (dados fictícios) |
| POST | `/api/pagamentos/simular` | Simula Pix/cartão, gera `Movimento` de crédito |

### Relatórios (D5)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/relatorios/vendas?inicio=&fim=` | Vendas por período |
| GET | `/api/relatorios/itens-mais-vendidos` | Ranking de itens |
| GET | `/api/relatorios/vendas/excel` | Export Excel (ClosedXML) |

---

## 9. Telas do frontend

### 9.1 Cadastro / Login (R1)
- **Login:** e-mail + senha. Redireciona conforme permissão
- **Cadastro de responsável:** nome, e-mail, CPF, telefone, senha
- **Cadastro de aluno:** feito pelo responsável logado — nome, matrícula, turma, data de nascimento, restrições alimentares. Cria `Usuario` com permissão `Aluno` e vincula ao adulto

### 9.2 Área do Aluno

**Menu / Home**
- Saldo em destaque (verde = crédito, vermelho = fiado com "R$ X de R$ 250")
- Botões grandes: *Fazer pedido* · *Meus pedidos* · *Meu extrato*
- Aviso se há pedido pendente de retirada, com o código

**Cardápio (R2 + R3)**
- Grid de itens: nome, **descrição**, preço, badge de alérgeno
- Item com `Estoque = 0` não aparece
- Item que conflita com a restrição do aluno: badge vermelho "Contém lactose" (D3)
- Seletor de intervalo (Manhã/Tarde) com contador: *"Fecha em 12 min"*
- Carrinho lateral com total em tempo real e saldo projetado
- Botão **Confirmar pedido** — desabilitado se estourar limite/teto, com a mensagem do motivo

**Meus pedidos**
- Lista por data/intervalo com status
- Pedido `Aberto`: botões *Alterar* e *Cancelar* (somem após o fechamento)
- Pedido `Confirmado`: exibe o **código de retirada** grande

**Meu extrato**
- Movimentos do mês, item a item
- Filtro por mês · botão exportar PDF

### 9.3 Área do Responsável

**Dashboard**
- Card por filho: nome, saldo, gasto do mês, limite diário
- Alerta de saldo baixo em destaque (D4)
- Fechamento em aberto, se houver

**Gestão do filho**
- Adicionar crédito (valor + método)
- Definir limite diário (D2)
- Editar restrições alimentares (D3)

**Extrato do filho (R7 / cena 4)**
- Timeline: data, itens comprados, quantidade, valor unitário, total
- Filtro por mês · export PDF

**Fechamento mensal**
- Consolidado do mês anterior, item a item
- Botão *Pagar* → gateway simulado (D1)

**Métodos de pagamento**
- Cadastro de Pix/cartão fictício, marcação do padrão

### 9.4 Área da Cantina

**Painel do intervalo (R8 / cena 2) — tela principal**
- Header: intervalo atual, contagem regressiva, total de pedidos
- **Coluna esquerda — Preparo:** consolidado por item (*"12x Pão de queijo"*) para a cozinha
- **Coluna direita — Pedidos:** cards com código, nome do aluno, itens, badge de alergia
- Busca no topo: código, nome ou matrícula (R4)
- Um toque no card → **Entregar**. Card sai da lista
- Filtro: Pendentes / Entregues

**PDV Balcão (R5 / cena 3) — otimizado para ~5s por atendimento**
- Grade de itens em botões grandes, agrupados por categoria, sem scroll na maioria dos casos
- Toque no item adiciona ao carrinho; toque de novo incrementa
- Campo de busca de aluno por nome/matrícula (opcional — venda avulsa é permitida)
- Ao selecionar o aluno: mostra saldo, limite e alerta de alergia
- Dois botões de finalização: **Lançar na conta** · **Pagou à vista**
- Se estourar o teto: bloqueia *Lançar na conta*, exibe **"Limite de R$ 250,00 atingido — somente à vista"** e mantém *Pagou à vista* habilitado (cena 5)
- Sem navegação: tudo em uma tela

**Gestão de itens**
- Tabela: nome, descrição, preço, estoque, alérgenos, ativo
- Edição inline de estoque e preço
- Criar/desativar item

**Relatórios (D5)**
- Vendas por período (gráfico + tabela)
- Ranking de itens mais vendidos
- Export Excel

---

## 10. Validação de pedido (`ValidacaoPedidoService`)

Todo pedido passa pelas seguintes checagens, em ordem:

1. **Janela de tempo** (só `Antecipado`) — agora < `HoraInicio - MinutosAntecedencia`; senão: *"Pedidos para este intervalo já fecharam"*
2. **Estoque** — todo item com `Estoque >= Quantidade`; senão: *"{Item} esgotou"*
3. **Pedido duplicado** — já existe pedido do aluno neste intervalo/data? Então altera em vez de criar
4. **Limite diário (D2)** — `gastoDoDia + total <= LimiteDiario`; senão: *"Limite diário de R$ X atingido"*
5. **Teto de fiado** — `saldo - total >= -250`; senão: *"Limite de R$ 250,00 atingido — somente à vista"*

`AVista` e `Online` pulam as validações 4 e 5 — dinheiro na hora não afeta a conta.

### Transação de confirmação (`ContaService`)
Dentro de uma única transação:
1. Cria `Pedido` + `ItemPedido` (com preço congelado)
2. Decrementa `Item.Estoque`
3. Cria `Movimento` do tipo `Compra`
4. Atualiza `Aluno.Saldo` / `Adulto.Saldo`
5. Grava `SaldoApos` no movimento

Se qualquer passo falhar, nada é gravado.

---

## 11. Modo de contingência (D6)

> *"E se a internet cair, a cantina não pode parar."*

**Implementação mínima viável:**
- Painel e PDV guardam em `localStorage` ao carregar: cardápio, lista de alunos (nome, matrícula, saldo) e pedidos do intervalo
- Sem conexão: interface entra em **modo offline** (faixa amarela no topo), continua permitindo marcar entrega e registrar venda no balcão
- Vendas offline entram numa fila local (`pendentes[]`)
- Ao voltar a conexão: `POST /api/pedidos/sincronizar` envia a fila; o servidor reprocessa validações e retorna quais foram aceitas
- Conflito (ex.: teto estourado offline): venda é registrada como `AVista` e sinalizada para conferência

**Prioridade:** só implementar depois que as 5 cenas estiverem funcionando.

---

## 12. Massa de teste (`SeedData`)

Nenhum dado real — tudo fictício.

- **2 intervalos:** Manhã (09:00–09:20), Tarde (15:30–15:50)
- **38 itens** com nome, descrição, preço, estoque e alérgenos
- **~20 alunos** distribuídos entre **~10 responsáveis**, incluindo:
  - 1 aluno com saldo positivo
  - 1 aluno próximo ao teto (ex.: saldo −R$ 240) → usado na **cena 5**
  - 1 aluno com restrição de lactose → usado na demo de **D3**
  - 1 aluno com limite diário baixo → usado na demo de **D2**
- **Histórico do mês anterior** para que o extrato (cena 4) e o fechamento tenham conteúdo real
- **Pedidos já confirmados** no intervalo atual para o painel (cena 2) abrir populado

---

## 13. Roteiro da demonstração (as 5 cenas)

| # | Cena | O que precisa acontecer na tela |
|---|---|---|
| 1 | **O aluno pede antes** | Escolhe itens do cardápio, define intervalo e confirma dentro do limite |
| 2 | **A cantina entrega** | Painel lista os pedidos do intervalo; atendente localiza e marca como entregue |
| 3 | **Alguém compra no balcão** | Aluno sem pedido antecipado é atendido: itens lançados e venda registrada em poucos toques |
| 4 | **O responsável confere** | Abre extrato do filho e vê o que foi comprado, quando e quanto |
| 5 | **O limite segura** | Pedido que estoura o teto de R$ 250 é recusado, com a mensagem certa |

> Estas 5 cenas **são o escopo**. Qualquer funcionalidade que não aparece nelas é secundária.

---

## 14. Ordem de implementação sugerida (13h de código)

| Fase | Tempo | Entrega |
|---|---|---|
| **0 — Setup** | 30 min | Monorepo, projetos criados, CORS, DbContext, migration inicial, seed rodando |
| **1 — Núcleo** | 3h | Entidades, Auth, Itens/Cardápio, Pedido antecipado com validações → **cena 1** |
| **2 — Cantina** | 3h | Painel do intervalo + entrega + PDV balcão → **cenas 2 e 3** |
| **3 — Responsável** | 2h | Extrato, crédito, limite → **cena 4** |
| **4 — Limites** | 1h | Teto R$ 250 e mensagens de bloqueio → **cena 5** |
| **5 — Desejáveis** | 2h | D3 alergias, D2 limite diário, D1 pagamento simulado, D5 relatórios |
| **6 — Integração** | 1h | Teste de ponta a ponta das 5 cenas, ajustes |
| **7 — Pitch** | 30 min | Ensaio da apresentação |

**Buffer implícito:** se alguma fase atrasar, D6 (contingência) e D4 (notificação) são as primeiras a cair.

---

## 15. Setup

```bash
# backend
cd backend
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run

# frontend
cd frontend
npm install
npm run dev
```

**Connection string** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=cantina;User=root;Password=SUA_SENHA;"
  }
}
```

**Frontend** (`.env`):
```
VITE_API_URL=http://localhost:5000/api
```
