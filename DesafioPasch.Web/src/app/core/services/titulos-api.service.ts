import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TituloEmAtraso, TitulosAtrasadosQuery } from '../models/titulo-em-atraso.model';

@Injectable({ providedIn: 'root' })
export class TitulosApiService {
  // Note: /api é o prefixo do proxy (rewritten para backend)
  private readonly baseUrl = '/api/v1/titulos';

  constructor(private http: HttpClient) {}

  listarAtrasados(query?: TitulosAtrasadosQuery): Observable<TituloEmAtraso[]> {
    let params = new HttpParams();

    if (query) {
      for (const [key, value] of Object.entries(query)) {
        if (value === undefined || value === null || value === '') continue;
        params = params.set(key, String(value));
      }
    }

    return this.http.get<TituloEmAtraso[]>(`${this.baseUrl}/atrasados`, { params });
  }
}
