import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { TitulosApiService } from '../../core/services/titulos-api.service';
import { TituloEmAtraso, TitulosAtrasadosQuery } from '../../core/models/titulo-em-atraso.model';

// Material
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ChangeDetectorRef } from '@angular/core';


@Component({
  selector: 'app-titulos-atrasados',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,

    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatProgressBarModule,
    MatSnackBarModule
  ],
  templateUrl: './titulos-atrasados.component.html',
  styleUrls: ['./titulos-atrasados.component.scss']
})
export class TitulosAtrasadosComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  loading = false;
  error?: string;

  itens: TituloEmAtraso[] = [];

  // table
  displayedColumns: Array<keyof TituloEmAtraso | 'acoes'> = [
    'numeroTitulo',
    'nomeDevedor',
    'quantidadeParcelas',
    'valorOriginal',
    'diasEmAtraso',
    'valorAtualizado',
    'multa',
    'jurosTotais'
  ];

  // pagination (client-side)
  pageIndex = 0;
  pageSize = 10;
  pageSizeOptions = [10, 20, 50];

  // sort (server-side)
  sortBy: TitulosAtrasadosQuery['sortBy'] = 'diasEmAtraso';
  sortDir: TitulosAtrasadosQuery['sortDir'] = 'desc';

  form: FormGroup;

  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;

  constructor(
    private api: TitulosApiService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private snack: MatSnackBar,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      numeroTitulo: [''],
      nomeDevedor: [''],
      minDiasAtraso: [null],
      maxDiasAtraso: [null],
      minValorAtualizado: [null],
      maxValorAtualizado: [null]
    });
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.itens.length / this.pageSize));
  }

  get pagedItens(): TituloEmAtraso[] {
    const start = this.pageIndex * this.pageSize;
    return this.itens.slice(start, start + this.pageSize);
  }

  ngOnInit(): void {
    // estado inicial vindo da URL
    this.route.queryParamMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((p) => {
        this.form.patchValue(
          {
            numeroTitulo: p.get('numeroTitulo') ?? '',
            nomeDevedor: p.get('nomeDevedor') ?? '',
            minDiasAtraso: this.toNumberOrNull(p.get('minDiasAtraso')),
            maxDiasAtraso: this.toNumberOrNull(p.get('maxDiasAtraso')),
            minValorAtualizado: this.toNumberOrNull(p.get('minValorAtualizado')),
            maxValorAtualizado: this.toNumberOrNull(p.get('maxValorAtualizado'))
          },
          { emitEvent: false }
        );

        this.sortBy = (p.get('sortBy') as any) ?? 'diasEmAtraso';
        this.sortDir = (p.get('sortDir') as any) ?? 'desc';

        this.pageIndex = 0;
        this.buscar();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onBuscarClick(): void {
    this.pageIndex = 0;
    this.paginator?.firstPage();
    this.syncUrl();
    this.buscar();
  }

  onLimparClick(): void {
    this.form.reset();
    this.sortBy = 'diasEmAtraso';
    this.sortDir = 'desc';

    this.pageIndex = 0;
    this.paginator?.firstPage();

    this.syncUrl();
    this.buscar();
  }

  onPage(ev: PageEvent): void {
    this.pageIndex = ev.pageIndex;
    this.pageSize = ev.pageSize;
  }

  onSortChange(ev: Sort): void {
    if (!ev.active) return;

    // map column -> sortBy permitido pelo backend
    const allowed: Array<TitulosAtrasadosQuery['sortBy']> = [
      'numeroTitulo',
      'nomeDevedor',
      'valorOriginal',
      'diasEmAtraso',
      'valorAtualizado'
    ];

    const col = ev.active as TitulosAtrasadosQuery['sortBy'];
    if (!allowed.includes(col)) return;

    this.sortBy = col;
    this.sortDir = (ev.direction || 'asc') as any;

    this.pageIndex = 0;
    this.paginator?.firstPage();

    this.syncUrl();
    this.buscar();
  }

  buscar(): void {
  queueMicrotask(() => this.loading = true);
  this.error = undefined;

  const v = this.form.getRawValue();

  const query: TitulosAtrasadosQuery = {
    numeroTitulo: v.numeroTitulo?.trim() || undefined,
    nomeDevedor: v.nomeDevedor?.trim() || undefined,
    minDiasAtraso: v.minDiasAtraso ?? undefined,
    maxDiasAtraso: v.maxDiasAtraso ?? undefined,
    minValorAtualizado: v.minValorAtualizado ?? undefined,
    maxValorAtualizado: v.maxValorAtualizado ?? undefined,
    sortBy: this.sortBy,
    sortDir: this.sortDir
  };

  this.api.listarAtrasados(query).subscribe({
    next: (data) => {
      this.itens = data;
      queueMicrotask(() => this.loading = false);
    },
    error: (err) => {
      console.error(err);
      this.error = 'Falha ao carregar títulos.';
      queueMicrotask(() => this.loading = false);
    }
  });
}

  private syncUrl(): void {
    const v = this.form.getRawValue();

    const qp: any = {};
    if (v.numeroTitulo) qp.numeroTitulo = v.numeroTitulo;
    if (v.nomeDevedor) qp.nomeDevedor = v.nomeDevedor;
    if (v.minDiasAtraso != null) qp.minDiasAtraso = v.minDiasAtraso;
    if (v.maxDiasAtraso != null) qp.maxDiasAtraso = v.maxDiasAtraso;
    if (v.minValorAtualizado != null) qp.minValorAtualizado = v.minValorAtualizado;
    if (v.maxValorAtualizado != null) qp.maxValorAtualizado = v.maxValorAtualizado;

    qp.sortBy = this.sortBy;
    qp.sortDir = this.sortDir;

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: qp,
      replaceUrl: true
    });
  }

  private toNumberOrNull(v: string | null): number | null {
    if (!v) return null;
    const n = Number(v);
    return Number.isFinite(n) ? n : null;
  }
}
