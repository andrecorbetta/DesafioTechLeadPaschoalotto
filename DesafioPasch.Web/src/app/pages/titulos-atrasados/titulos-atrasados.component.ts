import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';

import { TitulosApiService } from '../../core/services/titulos-api.service';
import { TituloEmAtraso, TitulosAtrasadosQuery } from '../../core/models/titulo-em-atraso.model';

// ViewModel para reduzir custo de pipes no template (moeda já formatada no TS)
export type TituloEmAtrasoVm = TituloEmAtraso & {
  valorOriginalFmt: string;
  valorAtualizadoFmt: string;
  multaFmt: string;
  jurosTotaisFmt: string;
};

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
import { MatSnackBarModule } from '@angular/material/snack-bar';


@Component({
  selector: 'app-titulos-atrasados',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
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

  private readonly currency = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  });

  loading = false;
  error?: string;

  itens: TituloEmAtrasoVm[] = [];

  // importante: NÃO use getter que recria array no template.
  // recalculamos a página somente quando dados/página mudarem.
  pagedItens: TituloEmAtrasoVm[] = [];

  // table
  displayedColumns: string[] = [
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
        this.paginator?.firstPage();
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
  }

  onLimparClick(): void {
    this.form.reset();
    this.sortBy = 'diasEmAtraso';
    this.sortDir = 'desc';

    this.pageIndex = 0;
    this.paginator?.firstPage();

    this.syncUrl();
  }

  onPage(ev: PageEvent): void {
    this.pageIndex = ev.pageIndex;
    this.pageSize = ev.pageSize;
    this.applyPaging();
    this.cdr.markForCheck();
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
  }

  buscar(): void {
    queueMicrotask(() => (this.loading = true));
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
        this.itens = this.mapToVm(data);
        this.pageIndex = 0;
        this.paginator?.firstPage();
        this.applyPaging();
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error(err);
        this.error = 'Falha ao carregar títulos.';
        this.applyPaging();
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  
private fmtCurrency(v: number | null | undefined): string {
  if (v === null || v === undefined) return '—';
  const n = Number(v);
  if (!Number.isFinite(n)) return '—';
  return this.currency.format(n);
}

private mapToVm(data: TituloEmAtraso[]): TituloEmAtrasoVm[] {
  return data.map((x) => ({
    ...x,
    valorOriginalFmt: this.fmtCurrency(x.valorOriginal),
    valorAtualizadoFmt: this.fmtCurrency(x.valorAtualizado),
    multaFmt: this.fmtCurrency(x.multa),
    jurosTotaisFmt: this.fmtCurrency(x.jurosTotais)
  }));
}

private applyPaging(): void {
    const start = this.pageIndex * this.pageSize;
    this.pagedItens = this.itens.slice(start, start + this.pageSize);
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
