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
| Itens no cardápio | 38 no briefing (34 na massa de teste, sem combos) |
| Pessoas atendendo | 3 (só 1 opera o sistema, no caixa, com a conta Admin) |

**Implicação de projeto:** ~5 segundos por atendimento no balcão. A tela do balcão é a parte mais crítica do sistema — deve funcionar com poucos toques, sem navegação entre páginas, com busca rápida por nome/código.

---

## 2. Atores

| Ator | Quem é | O que faz |
|---|---|---|
| **Aluno** | 11 a 18 anos | Vê cardápio, faz pedido antecipado, compra no balcão, vê o próprio saldo e pedidos |
| **Adulto (responsável)** | Pai, mãe ou tutor | Coloca crédito, define limite diário, vê extrato do filho, define método de pagamento, paga fechamento mensal |
| **Admin (cantina)** | 1 conta única, usada no caixa | Vê pedidos do intervalo, entrega, registra venda no balcão, gerencia itens/estoque, vê relatórios |

---

## 3. Requisitos — rastreabilidade

### Obrigatórios

| ID | Requisito | Onde é atendido |
|---|---|---|
| R1 | Cadastro de alunos e responsáveis, com vínculo | `UsuariosController`, `AlunosController`, `AdultosController` — telas de Cadastro e Gestão de Filhos |
| R2 | Cardápio do dia com itens, preços e disponibilidade | `DispCardapio` + `ItensController.GetCardapio()` — tela Cardápio |
| R3 | Pedido antecipado (monta pedido + escolhe intervalo) | `PedidosController.Create()` com `TipoVenda = Antecipado` |
| R4 | Retirada identificada por código, senha ou nome | `Pedido.CodigoRetirada` + busca no Painel do Admin |
| R5 | Venda direta no balcão, poucos toques, lançamento na conta ou pagamento na hora | `PedidosController.CreateBalcao()` — tela PDV Balcão |
| R6 | Lançamento na conta do aluno a cada compra | `Conta` + `Movimento` na mesma transação |
| R7 | Extrato e fechamento mensal, com histórico de consumo | `ExtratosController`, `FechamentosController` — tela Extrato |
| R8 | Painel da cantina com pedidos do próximo intervalo | `PainelController.GetIntervaloAtual()` — tela Painel |

### Desejáveis

| ID | Requisito | Onde é atendido |
|---|---|---|
| D1 | Pagamento online (Pix ou cartão), simulado | `PagamentosController` — serviço `GatewaySimuladoService` |
| D2 | Limite de gasto diário definido pelo responsável | `Aluno.LimiteDiario` + validação em `ValidacaoPedidoService` |
| D3 | Alergias e restrições alimentares sinalizadas no pedido | `Item.Alergenos` + `Aluno.RestricoesAlimentares` + alerta no pedido e no painel |
| D4 | Aviso de saldo baixo para o responsável | Badge na tela a partir do saldo (`NotificacaoService` com e-mail/WhatsApp não implementado) |
| D5 | Relatório de vendas por período e itens mais vendidos | `RelatoriosController` + export Excel/PDF |
| D6 | Modo de contingência para quando a internet cair | Cache local do painel/PDV + fila de sincronização |

---

## 4. Regras de negócio

| Regra | Implementação |
|---|---|
| **O balcão nunca fecha** | Pedido antecipado é caminho a mais, não o único. `TipoVenda = Balcao` sempre disponível, independente de horário |
| **Fiado tem teto** | Vale só para **aluno**: `Conta.Saldo` do aluno pode ficar negativo até **R$ 250,00**. Passou disso, só à vista. Validado em `ValidacaoPedidoService` |
| **Pedido fecha antes** | Pedidos antecipados fecham **15 minutos antes** do intervalo começar. Depois disso, cozinha já está montando |
| **Um pedido por intervalo** | Um `Pedido` por aluno por intervalo. Incluir mais itens = alterar o pedido existente enquanto estiver `Aberto` |
| **Cancelar só antes do fechamento** | Depois que fecha, sem cancelamento nem estorno. Pedidos já feitos são honrados |
| **Item esgotado some** | Item sem oferta em `DispCardapio` ou com `Estoque = 0` sai do cardápio do intervalo, mas pedidos já confirmados continuam válidos. Estoque é decrementado na **confirmação**, não na entrega |
| **O limite é do responsável** | Aluno não altera o próprio limite de gasto nem movimenta diretamente sua `Conta`. Bloqueio por permissão |
| **Fechamento no dia 1º** | Cada responsável recebe o consolidado do mês anterior, item a item |
| **Um único Admin** | Existe só uma conta `Admin`, criada pelo `SeedData`. O registro só cria `Aluno` ou `Adulto` — não há rota nem tela para criar outro Admin. Só uma pessoa opera o sistema no caixa |

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
- Conversão entidade → DTO feita à mão (métodos `ParaDto`), sem AutoMapper
- **ClosedXML 0.104.1** (export Excel)
- **QuestPDF 2024.10.3** (export PDF)
- **Swashbuckle.AspNetCore 6.6.2** (Swagger)
- **Autenticação:** cookie auth nativo do ASP.NET Core (`AddAuthentication().AddCookie()`) + `PasswordHasher<Usuario>` — ambos no framework, sem pacote extra. Permissão vai como claim de role → `[Authorize(Roles = "Adulto")]`

### Frontend
- **React + Vite**
- `fetch` para consumo da API
- **Proxy do Vite** (`server.proxy['/api']` → backend): front e API ficam na mesma origem, o cookie de sessão funciona sem CORS

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
| Permissao | enum | `Aluno` \| `Adulto` \| `Admin` |
| Ativo | bool | |
| CriadoEm | DateTime | |

### Aluno

| Campo | Tipo | Observação |
|---|---|---|
| UsuarioId | int | PK / FK → Usuario |
| AdultoId | int | FK → Adulto, **obrigatório** — todo aluno tem um responsável (N alunos → 1 adulto) |
| Turma | string | |
| DataNascimento | DateOnly | pedida no cadastro (9.1) |
| LimiteDiario | decimal? | definido pelo responsável (D2). Null = sem limite |
| RestricoesAlimentares | string | lista de alérgenos, separada por vírgula (D3) |

### Adulto

| Campo | Tipo | Observação |
|---|---|---|
| UsuarioId | int | PK / FK → Usuario |
| Cpf | string | |
| Telefone | string | usado no WhatsApp (D4) |
| MetodoPagamentoPadraoId | int? | FK → MetodoPagamento |

### Conta

Conta financeira do aluno ou adulto; centraliza o saldo que antes estava nas duas entidades.

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| UsuarioId | int | FK única → Usuario (uma conta por usuário comprador) |
| Saldo | decimal | positivo = crédito; negativo = fiado (mínimo -250 para o aluno; adulto sem limite diário nem teto) |
| Ativa | bool | |
| AtualizadaEm | DateTime | |

Toda alteração de `Saldo` cria `Movimento` na mesma transação. Compra paga à vista não debita a conta.

### Item

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| Nome | string | |
| **Descricao** | string | descrição do item exibida no cardápio |
| PrecoUnitario | decimal | |
| Estoque | int | 0 = esgotado, some do cardápio |
| Categoria | enum | `Salgado` \| `Doce` \| `Bebida` (combos foram retirados do escopo) |
| Alergenos | string | `Gluten,Lactose,Amendoim...` (D3) |
| Ativo | bool | desativa sem apagar histórico |

### DispCardapio

Oferta de itens por data e intervalo, separada do estoque global de `Item`.

| Campo | Tipo | Observação |
|---|---|---|
| Data | DateOnly | PK composta |
| IntervaloId | int | PK composta / FK → Intervalo |
| ItemId | int | PK composta / FK → Item |
| Disponivel | bool | indica se o item é oferecido naquele intervalo |

O cardápio mostra somente itens ativos, disponíveis em `DispCardapio` e com `Item.Estoque > 0`. No balcão fora de um intervalo, a regra atual continua permitindo item ativo com estoque.

### Pedido

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| UsuarioId | int | FK → Usuario (aluno **ou** adulto), **obrigatório** — inclusive na venda de balcão |
| Data | DateOnly | |
| IntervaloId | int? | FK → Intervalo. Null = venda de balcão fora de intervalo |
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
Extrato da `Conta` — alimenta extrato (R7) e fechamento.

| Campo | Tipo | Observação |
|---|---|---|
| Id | int | PK |
| ContaId | int | FK → Conta |
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
| ValorTotal | decimal | valor a pagar: o fiado (soma dos saldos negativos) das contas da família — responsável + filhos — na geração. O consumo do mês aparece só como informativo, para não cobrar de novo o que já foi pago com crédito |
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
│   ├── Conta.cs
│   ├── DispCardapio.cs
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
│       ├── CategoriaItem.cs
│       ├── TipoMetodoPagamento.cs
│       └── StatusFechamento.cs
│
├── DTOs/
│   ├── Auth/           (LoginDto, RegistroDto, UsuarioLogadoDto)
│   ├── Alunos/         (AlunoDto, CreateAlunoDto, LimiteDto, RestricoesDto, CreditoDto)
│   ├── Itens/          (ItemDto, SalvarItemDto, DisponibilidadeDto)
│   ├── Pedidos/        (PedidoDto, ItemPedidoDto, CriarPedidoDto, AlterarPedidoDto, VendaBalcaoDto, ItemQuantidadeDto, ResultadoSincronizacaoDto)
│   ├── Painel/         (PainelDto, PreparoItemDto)
│   ├── Extratos/       (ExtratoDto, MovimentoDto)
│   ├── Fechamentos/    (FechamentoDto, ConsumoDto, ConsumoItemDto)
│   ├── Pagamentos/     (MetodoPagamentoDto, CreateMetodoPagamentoDto, SimularPagamentoDto)
│   └── Relatorios/     (VendaDiaDto, ItemVendidoDto)
│
├── Data/
│   ├── AppDbContext.cs
│   ├── SeedData.cs             ← massa de teste fictícia
│   └── Migrations/
│
├── Mappings/
│   └── PedidoMapper.cs              ← Pedido → PedidoDto (Pedidos e Painel)
│
├── Extensions/
│   ├── ClaimsPrincipalExtensions.cs ← User.UsuarioId() lido do cookie
│   └── AcessoExtensions.cs          ← quem pode ver um aluno (ele, o responsável, o Admin)
│
├── Services/
│   ├── PedidoService.cs             ← janela, disponibilidade, estoque, limite diário, teto R$250, criar/alterar/cancelar/entregar, balcão, sincronizar (D6)
│   ├── ContaService.cs              ← Movimento + Conta.Saldo; crédito simulado (D1)
│   ├── FechamentoService.cs         ← gera e paga o fechamento mensal
│   ├── Alergia.cs                   ← conflito de alérgenos (D3), funções estáticas
│   └── RegraException.cs            ← regra violada → 400 com a mensagem (tratada no Program.cs)
│
├── Reports/
│   ├── ExcelExportService.cs        ← ClosedXML
│   └── PdfExportService.cs          ← QuestPDF
│
├── appsettings.json                 (vai pro Git, sem segredo)
├── appsettings.Development.json     (não vai pro Git: connection string)
├── appsettings.Example.json         (vai pro Git: modelo)
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
| POST | `/api/auth/login` | Valida senha, cria cookie de sessão, retorna permissão |
| POST | `/api/auth/logout` | Encerra a sessão |
| GET | `/api/auth/me` | Dados do usuário logado |

### Alunos
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/alunos` | Lista (filtro `?busca=` por nome/e-mail — usado no balcão) |
| GET | `/api/alunos/{id}` | Detalhe com saldo da `Conta` e restrições |
| POST | `/api/alunos` | Cadastra aluno vinculado a um adulto — R1 |
| GET | `/api/adultos/{id}/filhos` | Filhos do responsável |
| PUT | `/api/alunos/{id}/limite` | Define limite diário — D2 (só Adulto) |
| PUT | `/api/alunos/{id}/restricoes` | Define alergias — D3 |
| POST | `/api/alunos/{id}/credito` | Responsável adiciona crédito |

### Intervalos
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/intervalos` | Intervalos com horário de início, fim e fechamento dos pedidos (seletor do cardápio) |

### Itens / Cardápio
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/itens/cardapio?data=&intervaloId=` | Cardápio: oferta em `DispCardapio`, item ativo e `Estoque > 0` — R2 |
| GET | `/api/itens` | Todos (gestão do Admin) |
| POST | `/api/itens` | Cria item |
| PUT | `/api/itens/{id}` | Edita nome, descrição, preço, estoque, categoria e alérgenos (também usado na edição inline) |
| PUT | `/api/cardapio/{data}/{intervaloId}/{itemId}` | Disponibiliza ou retira item no dia/intervalo |
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

### Painel do Admin (R8)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/painel/intervalo-atual` | Pedidos do próximo intervalo, agrupados |
| GET | `/api/painel/preparo` | Consolidado por item: quanto precisa ser preparado |
| GET | `/api/painel/buscar?termo=` | Busca por código, nome ou e-mail — R4 |

### Extratos e fechamento (R7)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/extratos/aluno/{id}?mes=` | Extrato item a item |
| GET | `/api/extratos/aluno/{id}/pdf` | Export PDF (QuestPDF) |
| GET | `/api/fechamentos/adulto/{id}` | Fechamentos do responsável |
| POST | `/api/fechamentos/gerar` | Gera consolidado do mês anterior (dia 1º) |
| POST | `/api/fechamentos/{id}/pagar` | Paga fechamento (simulado, D1): quita o fiado das contas da família, da mais negativa para a menos; sobra vira crédito do responsável |

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
- **Cadastro de aluno:** feito pelo responsável logado — nome, e-mail institucional (obrigatório, domínio do instituto), turma, data de nascimento, restrições alimentares. Cria `Usuario` com permissão `Aluno` e vincula ao adulto

### 9.2 Área do Aluno

**Menu / Home**
- Saldo em destaque (verde = crédito, vermelho = fiado com "R$ X de R$ 250")
- Botões grandes: *Fazer pedido* · *Meus pedidos* · *Meu extrato*
- Aviso se há pedido pendente de retirada, com o código

**Cardápio (R2 + R3)**
- Grid de itens: nome, **descrição**, preço, badge de alérgeno
- Item sem oferta em `DispCardapio` ou com `Estoque = 0` não aparece
- Item que conflita com a restrição do aluno: badge vermelho "Contém lactose" (D3)
- Seletor de intervalo (Manhã/Tarde) com contador: *"Fecha em 12 min"*
- Carrinho lateral com total em tempo real e saldo projetado da `Conta`
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

### 9.4 Área do Admin (cantina)

**Painel do intervalo (R8 / cena 2) — tela principal**
- Header: intervalo atual, contagem regressiva, total de pedidos
- **Coluna esquerda — Preparo:** consolidado por item (*"12x Pão de queijo"*) para a cozinha
- **Coluna direita — Pedidos:** cards com código, nome do aluno, itens, badge de alergia
- Busca no topo: código, nome ou e-mail (R4)
- Um toque no card → **Entregar**. Card sai da lista
- Filtro: Pendentes / Entregues

**PDV Balcão (R5 / cena 3) — otimizado para ~5s por atendimento**
- Grade de itens em botões grandes, agrupados por categoria, sem scroll na maioria dos casos
- Toque no item adiciona ao carrinho; toque de novo incrementa
- Campo de busca de usuário por nome/e-mail — **obrigatório**: toda venda fica ligada a um usuário. A venda de balcão não entra na fila do painel: nasce `Entregue`
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

## 10. Validação de pedido (`PedidoService`)

Todo pedido passa pelas seguintes checagens, em ordem:

1. **Janela de tempo** (só `Antecipado`) — agora < `HoraInicio - MinutosAntecedencia`; senão: *"Pedidos para este intervalo já fecharam"*
2. **Disponibilidade e estoque** — item oferecido em `DispCardapio` para data/intervalo (não vale no balcão) e com `Estoque >= Quantidade`; senão: *"{Item} não está no cardápio deste intervalo"* ou *"{Item} esgotou"*
3. **Pedido duplicado** (só `Antecipado`) — já existe pedido do usuário neste intervalo/data? Recusa com *"Você já tem um pedido neste intervalo. Altere o pedido existente."*
4. **Limite diário (D2)** — `gastoDoDia + total <= LimiteDiario`; senão: *"Limite diário de R$ X atingido"*
5. **Teto de fiado** — `saldo - total >= -250`; senão: *"Limite de R$ 250,00 atingido — somente à vista"*

`AVista` e `Online` pulam as validações 4 e 5 — dinheiro na hora não afeta a conta.
Pedido de **adulto** também pula 4 e 5 — limite diário e teto de fiado são regras só do aluno.

### Gravação (`PedidoService`)
Num único `SaveChanges` (o EF executa numa transação):
1. Cria `Pedido` + `ItemPedido` (com preço congelado)
2. Decrementa `Item.Estoque`
3. Cria `Movimento` do tipo `Compra`
4. Atualiza `Conta.Saldo`
5. Grava `SaldoApos` no movimento

Se qualquer passo falhar, nada é gravado.
Se outra compra mudar o estoque ou o saldo ao mesmo tempo, nada é gravado e a resposta pede para tentar de novo (`Item.Estoque` e `Conta.Saldo` são tokens de concorrência).

### Ciclo do pedido antecipado
- **Criar:** debita a conta e baixa o estoque na hora; status `Aberto`
- **Alterar** (só `Aberto` e com a janela aberta): devolve o estoque antigo, reserva o novo e lança só a diferença (`Compra` se aumentou, `Estorno` se diminuiu)
- **Cancelar** (mesma condição): devolve o estoque, gera `Estorno` do total; status `Cancelado`
- **Confirmado:** não é gravado por job — quando a janela fecha, a API mostra o pedido `Aberto` como `Confirmado`
- **Entregar** (Admin, no painel): status `Entregue`
- **Balcão:** nasce `Entregue`, não passa pela janela nem pelo `DispCardapio`

---

## 11. Modo de contingência (D6)

> *"E se a internet cair, a cantina não pode parar."*

**Implementação mínima viável:**
- Painel e PDV guardam em `localStorage` ao carregar: cardápio, lista de alunos (nome, e-mail, saldo) e pedidos do intervalo
- Sem conexão: interface entra em **modo offline** (faixa amarela no topo), continua permitindo marcar entrega e registrar venda no balcão
- Vendas offline entram numa fila local (`pendentes[]`)
- Ao voltar a conexão: `POST /api/pedidos/sincronizar` envia a fila; o servidor reprocessa validações e retorna quais foram aceitas
- Conflito (ex.: teto estourado offline): venda é registrada como `AVista` e sinalizada para conferência

**Prioridade:** só implementar depois que as 5 cenas estiverem funcionando.

---

## 12. Massa de teste (`SeedData`)

Nenhum dado real — tudo fictício.

- **1 conta Admin** (única no sistema) para o caixa

- **2 intervalos:** Manhã (09:00–09:20), Tarde (15:30–15:50)
- **34 itens** (12 salgados, 10 doces, 12 bebidas) com nome, descrição, preço, estoque e alérgenos
- **`DispCardapio`** para os intervalos do dia da demonstração, cobrindo os itens oferecidos
- **~20 alunos** distribuídos entre **~10 responsáveis**, com `Conta` para cada comprador, incluindo:
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
| 2 | **A cantina entrega** | Painel lista os pedidos do intervalo; o Admin localiza e marca como entregue |
| 3 | **Alguém compra no balcão** | Aluno sem pedido antecipado é atendido: itens lançados e venda registrada em poucos toques |
| 4 | **O responsável confere** | Abre extrato do filho e vê o que foi comprado, quando e quanto |
| 5 | **O limite segura** | Pedido que estoura o teto de R$ 250 é recusado, com a mensagem certa |

> Estas 5 cenas **são o escopo**. Qualquer funcionalidade que não aparece nelas é secundária.

---

## 14. Ordem de implementação sugerida (13h de código)

| Fase | Tempo | Entrega |
|---|---|---|
| **0 — Setup** | 30 min | Monorepo, projetos criados, proxy do Vite, DbContext, migration com `Conta` e `DispCardapio`, seed rodando (com a conta Admin) |
| **1 — Núcleo** | 3h | Entidades, `Conta`, Auth, Itens/`DispCardapio`, pedido antecipado com validações → **cena 1** |
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

**Connection string** (`appsettings.Development.json`, copiado do `appsettings.Example.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=cantina;User=root;Password=SUA_SENHA;"
  }
}
```

**Frontend** (`.env`):
```
VITE_API_URL=/api
```

