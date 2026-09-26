import { api } from './api';

// Rota do IntervalosController: [{ id, nome, horaInicio: "09:00:00", horaFim, fechamento: "08:45:00" }]
export const intervaloService = {
  listar: () => api('/intervalos'),
};
