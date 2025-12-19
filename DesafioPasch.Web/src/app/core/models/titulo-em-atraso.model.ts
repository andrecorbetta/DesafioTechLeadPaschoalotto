export interface TituloEmAtraso {
  numeroTitulo: string;
  nomeDevedor: string;
  quantidadeParcelas: number;
  valorOriginal: number;
  diasEmAtraso: number;
  valorAtualizado: number;
  multa: number;
  jurosTotais: number;
}

export interface TitulosAtrasadosQuery {
  numeroTitulo?: string;
  nomeDevedor?: string;

  minValorAtualizado?: number;
  maxValorAtualizado?: number;

  minDiasAtraso?: number;
  maxDiasAtraso?: number;

  sortBy?: 'valorAtualizado' | 'diasEmAtraso' | 'nomeDevedor' | 'numeroTitulo' | 'valorOriginal';
  sortDir?: 'asc' | 'desc';
}
