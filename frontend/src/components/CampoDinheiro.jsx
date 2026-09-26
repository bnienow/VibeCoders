import { dinheiro } from '../services/formatos';

const MAXIMO_DE_DIGITOS = 7; // até R$ 99.999,99

// Campo de moeda no estilo caixa eletrônico: digita-se só números e os centavos entram pela direita
// (4 → R$ 0,04; 45 → R$ 0,45; 450 → R$ 4,50). Apagar tudo deixa o campo vazio.
// valor: número em reais, ou null quando vazio. aoMudar recebe o novo número (ou null).
export default function CampoDinheiro({ valor, aoMudar, ...props }) {
  function mudar(texto) {
    const centavos = texto.replace(/\D/g, '').slice(0, MAXIMO_DE_DIGITOS);
    aoMudar(centavos ? Number(centavos) / 100 : null);
  }

  return <input className="input" inputMode="numeric" value={valor == null ? '' : dinheiro(valor)} onChange={e => mudar(e.target.value)} {...props} />;
}
