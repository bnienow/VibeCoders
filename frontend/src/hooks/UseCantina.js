import { useSyncExternalStore } from 'react';
import { cantinaService } from '../services/cantinaService';
export function useCantina() {
  const estado = useSyncExternalStore(cantinaService.subscribe, cantinaService.snapshot, cantinaService.snapshot);
  return { estado, usuario: cantinaService.usuarioAtual(estado), itensCarrinho: cantinaService.itensDoCarrinho(estado), totalCarrinho: cantinaService.totalDoCarrinho(estado), service: cantinaService };
}
