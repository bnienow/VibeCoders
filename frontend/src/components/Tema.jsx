// Folha de estilos única do app
const css = `
/* ---------- Base ---------- */
:root { font-family: Inter, ui-sans-serif, system-ui, -apple-system, "Segoe UI", sans-serif; color: #17372f; background: #f7f8f2; font-synthesis: none; }
* { box-sizing: border-box; }
body { margin: 0; }
#root { min-height: 100vh; }
a { color: inherit; text-decoration: none; }
button, input, select, textarea { font: inherit; }
button { cursor: pointer; }
button:disabled { cursor: not-allowed; opacity: .48; }
h1, h2, h3, p { margin-top: 0; }
h1, h2 { color: #17372f; font-weight: 800; }
h1 { font-size: clamp(28px, 3.4vw, 40px); letter-spacing: -.04em; line-height: 1.1; margin-bottom: 0; }
h2 { font-size: clamp(19px, 2vw, 24px); letter-spacing: -.03em; margin-bottom: 14px; }
h3 { font-size: 16px; letter-spacing: -.02em; margin-bottom: 0; }
p { line-height: 1.5; }

/* ---------- Estrutura ---------- */
.container { width: min(1240px, calc(100% - 40px)); margin: auto; }
.shell { min-height: 100vh; display: flex; flex-direction: column; }
.main { flex: 1; padding: 36px 0 72px; }
.section-head { margin-bottom: 24px; }

.page-header { position: sticky; top: 0; z-index: 50; background: #fffdf9ed; border-bottom: 1px solid #e2e7dc; backdrop-filter: blur(16px); }
.header-inner { display: flex; align-items: center; justify-content: space-between; gap: 16px; min-height: 68px; flex-wrap: wrap; padding: 10px 0; }
.logo { font-weight: 900; color: #17503e; letter-spacing: -.04em; font-size: 20px; }
.logo span { color: #e96143; }
.role { font-size: 11px; font-weight: 800; text-transform: uppercase; letter-spacing: .1em; color: #688074; }
.header-actions { display: flex; align-items: center; gap: 12px; }
.site-nav { display: flex; flex-wrap: wrap; gap: 4px; margin-left: auto; }
.site-nav a { padding: 8px 13px; border-radius: 30px; color: #52675e; font-size: 13px; font-weight: 700; }
.site-nav a:hover { background: #eef4ef; }
.site-nav a.active { background: #e2efe8; color: #17503e; }

/* Faixa do modo offline (sem conexão com a API) */
.faixa-offline { background: #fff1d7; color: #6f4d0c; border-bottom: 1px solid #f0d9a8; padding: 12px 0; font-size: 14px; }
.faixa-offline.ok { background: #e8f5ee; color: #185a42; border-color: #cfe8da; }
.faixa-offline .atencao { color: #a13f2a; font-weight: 700; }

/* ---------- Layout ---------- */
.row { display: flex; align-items: center; gap: 12px; }
.wrap { flex-wrap: wrap; }
.between { display: flex; justify-content: space-between; align-items: center; gap: 16px; }
.stack { display: grid; gap: 16px; }
.grid-2 { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 20px; }
.grid-3 { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 18px; }
.align-start { align-items: start; }
.cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(230px, 1fr)); gap: 14px; }
.two-column { display: grid; grid-template-columns: minmax(0, 1.7fr) minmax(280px, .8fr); gap: 22px; align-items: start; }
.admin-grid { display: grid; grid-template-columns: minmax(240px, .7fr) minmax(0, 1.3fr); gap: 20px; align-items: start; }
.sticky-panel { position: sticky; top: 92px; }
.right { text-align: right; }

/* Barra de filtros: campos e botões alinhados pela base */
.toolbar { display: flex; flex-wrap: wrap; align-items: flex-end; gap: 12px; }
.toolbar .field { flex: 1 1 180px; }
.toolbar .btn { flex: 0 0 auto; }

/* ---------- Superfícies ---------- */
.surface { background: #fff; border: 1px solid #e6e8df; border-radius: 20px; box-shadow: 0 12px 35px #16372c0b; }
.pad { padding: 24px; }
.divider { height: 1px; background: #e9ece6; margin: 16px 0; }
.line { display: flex; justify-content: space-between; align-items: center; gap: 15px; padding: 12px 0; border-bottom: 1px solid #eef0eb; }
.line:last-child { border-bottom: 0; }
.kpi { padding: 20px 22px; }
.kpi span { color: #637d6e; font-size: 13px; }
.kpi strong { display: block; font-size: 28px; margin-top: 8px; letter-spacing: -.04em; }
.metric { font-size: 28px; font-weight: 850; letter-spacing: -.035em; }
.clock { font-variant-numeric: tabular-nums; }
.empty { padding: 32px; text-align: center; color: #657b71; background: #fff; border: 1px dashed #d7e1d8; border-radius: 18px; }
.note { padding: 14px 18px; border-radius: 14px; background: #eff6ed; color: #285f4a; }

/* Formulário em cartão: título, campos e botões sempre no rodapé (cartões lado a lado ficam alinhados) */
.card-form { display: flex; flex-direction: column; gap: 14px; }
.card-form h2 { margin: 0; }
.card-form .acoes { margin-top: auto; display: grid; gap: 10px; }

/* ---------- Texto ---------- */
.muted { color: #698078; }
.small { font-size: 13px; }
.hint { font-size: 12px; color: #71867b; margin: 0; }
.price { color: #1c664d; font-weight: 850; }

/* ---------- Botões ---------- */
.btn { display: inline-flex; align-items: center; justify-content: center; gap: 8px; min-height: 44px; padding: 10px 18px; border: 1px solid #e96143; border-radius: 12px; background: #e96143; color: #fff; font-weight: 800; white-space: nowrap; }
.btn:hover:not(:disabled) { filter: brightness(.94); }
.btn.secondary { background: #fff; color: #17483d; border-color: #cadbd2; }
.btn.soft { background: #e7f1eb; color: #205d4a; border-color: #e7f1eb; }
.btn.danger { background: #fff; color: #ae4231; border-color: #efcfc7; }
.btn.small { min-height: 36px; padding: 6px 12px; font-size: 13px; }
.btn.full { width: 100%; }
.text-button { border: 0; background: none; padding: 0; color: #e96143; font-weight: 800; }

.pill-tabs { display: flex; flex-wrap: wrap; gap: 8px; }
.pill-tabs button { border: 1px solid #d4e1d6; background: #fff; color: #33574a; border-radius: 30px; padding: 8px 15px; font-weight: 700; }
.pill-tabs button[aria-pressed=true] { background: #184c3d; color: #fff; border-color: #184c3d; }

/* ---------- Formulários ---------- */
.field { display: grid; gap: 7px; font-size: 13px; font-weight: 760; color: #29483e; }
.input { width: 100%; min-height: 44px; padding: 10px 13px; background: #fff; color: #17372f; border: 1px solid #ccd8cd; border-radius: 11px; outline: none; }
.input:focus { border-color: #2a846a; box-shadow: 0 0 0 3px #2a846a22; }
.input:disabled { background: #f3f6f3; }
textarea.input { resize: vertical; }
.form-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
.check { display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 600; color: #29483e; }

/* Campo com botão ou sufixo colado (senha + "Ver", e-mail + "@dominio") */
.input-group { display: flex; align-items: stretch; }
.input-group .input { border-radius: 11px 0 0 11px; min-width: 0; }
.input-group .addon { display: flex; align-items: center; padding: 0 12px; border: 1px solid #ccd8cd; border-left: 0; border-radius: 0 11px 11px 0; background: #f3f6f3; color: #52675e; font-size: 13px; font-weight: 700; white-space: nowrap; }
.input-group button.addon { background: #fff; color: #17483d; }

/* ---------- Mensagens ---------- */
.badge { display: inline-flex; align-items: center; border-radius: 100px; padding: 5px 11px; background: #e7f3eb; color: #1b7050; font-weight: 800; font-size: 12px; white-space: nowrap; }
.badge.red { background: #fde9e5; color: #b94230; }
.badge.amber { background: #fff1d7; color: #956319; }
.alert, .success { padding: 12px 16px; border-radius: 12px; font-size: 14px; line-height: 1.4; }
.alert { background: #fff0e9; color: #a13f2a; }
.success { background: #e8f5ee; color: #185a42; }

/* ---------- Itens do cardápio ---------- */
.product { padding: 16px; display: flex; flex-direction: column; gap: 8px; }
.product.chosen { border-color: #e96143; box-shadow: 0 0 0 2px #e9614322; }
.product p { font-size: 13px; color: #657b71; margin: 0; }
.product .bottom { margin-top: auto; padding-top: 8px; display: flex; align-items: center; justify-content: space-between; gap: 10px; }
.category { margin: 28px 0; }
.quantity { display: inline-flex; align-items: center; gap: 4px; border: 1px solid #d1ded3; border-radius: 12px; padding: 3px; background: #fff; white-space: nowrap; }
.quantity button { width: 32px; height: 32px; border: 0; border-radius: 8px; background: #e8f3ec; color: #1b6a4c; font-size: 20px; font-weight: 800; }
.quantity output { min-width: 26px; text-align: center; font-weight: 850; }
.tile { color: #17372f; text-align: left; min-height: 100px; display: grid; gap: 6px; align-content: start; }

/* ---------- Tabelas ---------- */
.table-scroll { overflow-x: auto; }
.data-table { border-collapse: collapse; width: 100%; min-width: 820px; }
.data-table th { text-align: left; color: #62756d; font-size: 12px; text-transform: uppercase; letter-spacing: .08em; padding: 12px; border-bottom: 1px solid #e5eae3; white-space: nowrap; }
.data-table td { padding: 12px; border-bottom: 1px solid #edf0eb; vertical-align: middle; }
.data-table tr:last-child td { border-bottom: 0; }
.data-table .input { min-height: 38px; padding: 6px 10px; }

/* ---------- Painel e relatórios ---------- */
.order-card { padding: 16px; margin: 12px 0; }
.bar { height: 12px; border-radius: 20px; background: #e6eee7; overflow: hidden; }
.bar i { display: block; height: 100%; background: #e96143; border-radius: 20px; }

/* ---------- Login e cadastro ---------- */
.auth { min-height: 100vh; display: grid; place-items: center; padding: 32px 16px; background: radial-gradient(circle at 15% 15%, #dcebdd, transparent 35%), #f8f8f1; }
.auth-card { width: min(100%, 470px); padding: 32px; }
.auth-card h1 { font-size: 28px; margin-bottom: 20px; }
.auth-card form { display: grid; gap: 16px; }
.auth-bottom { text-align: center; font-size: 13px; color: #698078; margin: 18px 0 0; }

/* ---------- Telas menores ---------- */
@media (max-width: 950px) {
  .two-column, .admin-grid { grid-template-columns: 1fr; }
  .sticky-panel { position: static; }
}
@media (max-width: 640px) {
  .container { width: calc(100% - 26px); }
  .main { padding: 24px 0 56px; }
  .grid-2, .grid-3, .form-grid { grid-template-columns: 1fr; }
  .pad { padding: 18px; }
  .auth-card { padding: 22px; }
  .site-nav { width: 100%; margin-left: 0; }
  .header-actions { width: 100%; justify-content: space-between; }
}
@media print {
  .page-header, .no-print { display: none !important; }
  .main { padding: 0; }
  .surface { box-shadow: none; }
  body { background: #fff; }
}
`;

export default function Tema() { return <style>{css}</style>; }
