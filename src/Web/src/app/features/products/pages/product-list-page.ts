import { DestroyRef, Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiError } from '../../../core/http/problem-details';
import { mapApiError } from '../../../core/http/api-error.mapper';
import { EmptyState } from '../../../shared/components/empty-state';
import { FeedbackMessage } from '../../../shared/components/feedback-message';
import { LoadingIndicator } from '../../../shared/components/loading-indicator';
import { ProductsApiService } from '../data-access/products-api.service';
import { Paged, ProductSummary } from '../models/product.models';
import { ProductCreateForm } from '../components/product-create-form';

@Component({
  imports: [EmptyState, FeedbackMessage, LoadingIndicator, ProductCreateForm],
  template: `
    <section>
      <div class="flex flex-wrap items-start justify-between gap-x-6 gap-y-4">
        <div><p class="text-sm font-semibold text-blue-700">Estoque</p><h1 class="mt-1 text-3xl font-bold tracking-tight">Produtos</h1><p class="mt-2 text-slate-600">Consulte os produtos cadastrados e seus saldos atuais.</p></div>
        <button type="button" (click)="openCreateDialog()" class="inline-flex items-center gap-2 self-start rounded-lg bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-700 sm:mt-6">
          <svg aria-hidden="true" viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="2" class="size-4"><path d="M10 4v12M4 10h12" stroke-linecap="round" /></svg>
          Novo produto
        </button>
      </div>

      <div class="mt-4 space-y-4">
        @if (created()) { <app-feedback-message kind="success" title="Produto cadastrado com sucesso." /> }
        @if (error(); as apiError) { <app-feedback-message kind="error" title="Não foi possível carregar os produtos." [message]="apiError.message" [traceId]="apiError.traceId" /> }
        @if (loading()) { <div class="rounded-xl border border-slate-200 bg-white p-6"><app-loading-indicator label="Carregando produtos..." /></div> }
        @else if (page(); as currentPage) {
          @if (currentPage.items.length === 0) { <app-empty-state title="Nenhum produto cadastrado" description="Cadastre o primeiro produto para utilizá-lo em uma nota fiscal." actionLabel="Cadastrar produto" (action)="openCreateDialog()" /> }
          @else {
            <div class="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
              <div class="overflow-x-auto">
                <table class="w-full border-collapse text-left text-sm">
                  <caption class="sr-only">Produtos cadastrados e saldos disponíveis</caption>
                  <thead class="bg-slate-100 text-slate-700"><tr><th scope="col" class="px-4 py-3 font-semibold">Código</th><th scope="col" class="px-4 py-3 font-semibold">Descrição</th><th scope="col" class="px-4 py-3 text-right font-semibold">Saldo</th></tr></thead>
                  <tbody class="divide-y divide-slate-200">
                    @for (product of currentPage.items; track product.id) { <tr><td class="whitespace-nowrap px-4 py-3 font-medium text-slate-900">{{ product.code }}</td><td class="px-4 py-3 text-slate-700">{{ product.description }}</td><td class="px-4 py-3 text-right font-semibold tabular-nums">@if (product.balance === 0) { <span class="inline-flex rounded-full bg-red-100 px-2.5 py-1 text-xs font-semibold text-red-800" aria-label="Saldo zero, sem estoque">Sem estoque</span> } @else { {{ product.balance }} }</td></tr> }
                  </tbody>
                </table>
              </div>
              <div class="flex flex-wrap items-center justify-between gap-3 border-t border-slate-200 px-4 py-3 text-sm text-slate-600">
                <p>{{ rangeLabel() }} de {{ currentPage.totalRecords }} produtos</p>
                <nav class="flex gap-2" aria-label="Paginação de produtos">
                  <button type="button" (click)="previousPage()" [disabled]="currentPage.pageNumber <= 1 || loading()" class="rounded-md border border-slate-300 px-3 py-2 font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50">Anterior</button>
                  <span class="flex items-center px-2" aria-current="page">Página {{ currentPage.pageNumber }} de {{ currentPage.totalPages }}</span>
                  <button type="button" (click)="nextPage()" [disabled]="currentPage.pageNumber >= currentPage.totalPages || loading()" class="rounded-md border border-slate-300 px-3 py-2 font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50">Próxima</button>
                </nav>
              </div>
            </div>
          }
        }
      </div>

      <dialog #createDialog (cancel)="closeCreateDialog($event)" class="m-auto w-[calc(100%-2rem)] max-w-2xl rounded-xl bg-white p-0 text-slate-950 shadow-2xl backdrop:bg-slate-950/55">
        <div class="max-h-[calc(100vh-2rem)] overflow-y-auto p-6 sm:p-8">
          <div class="mb-6 flex items-start justify-between gap-4">
            <div><p class="text-sm font-semibold text-blue-700">Estoque</p><h2 id="create-product-title" class="mt-1 text-2xl font-bold tracking-tight">Novo produto</h2><p class="mt-2 text-sm text-slate-600">Cadastre os dados que serão utilizados nas notas fiscais.</p></div>
            <button type="button" (click)="closeCreateDialog()" class="rounded-md p-2 text-2xl leading-none text-slate-500 hover:bg-slate-100 hover:text-slate-800" aria-label="Fechar cadastro de produto">×</button>
          </div>
          @if (createDialogOpen()) {
            <app-product-create-form (created)="onProductCreated()" (cancelled)="closeCreateDialog()" />
          }
        </div>
      </dialog>
    </section>
  `,
})
export class ProductListPage {
  private readonly api = inject(ProductsApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly pageSize = 20;
  private readonly createDialog = viewChild.required<ElementRef<HTMLDialogElement>>('createDialog');
  private createDialogOpener: HTMLElement | null = null;

  protected readonly page = signal<Paged<ProductSummary> | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<ApiError | null>(null);
  protected readonly created = signal(this.route.snapshot.queryParamMap.get('created') === 'true');
  protected readonly createDialogOpen = signal(false);
  protected readonly rangeLabel = computed(() => {
    const current = this.page();
    if (!current || current.totalRecords === 0) return '0';
    const start = (current.pageNumber - 1) * current.pageSize + 1;
    const end = Math.min(current.pageNumber * current.pageSize, current.totalRecords);
    return `${start}–${end}`;
  });

  constructor() { this.loadPage(1); }

  protected previousPage(): void { const current = this.page(); if (current && current.pageNumber > 1) this.loadPage(current.pageNumber - 1); }
  protected nextPage(): void { const current = this.page(); if (current && current.pageNumber < current.totalPages) this.loadPage(current.pageNumber + 1); }
  protected openCreateDialog(): void { this.createDialogOpener = document.activeElement as HTMLElement | null; this.created.set(false); this.createDialogOpen.set(true); const dialog = this.createDialog().nativeElement; dialog.showModal(); queueMicrotask(() => dialog.querySelector<HTMLElement>('input')?.focus()); }
  protected closeCreateDialog(event?: Event): void { event?.preventDefault(); this.createDialog().nativeElement.close(); this.createDialogOpen.set(false); this.createDialogOpener?.focus(); this.createDialogOpener = null; }
  protected onProductCreated(): void { this.closeCreateDialog(); this.created.set(true); this.loadPage(1); }

  private loadPage(pageNumber: number): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list(pageNumber, this.pageSize).pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (page) => this.page.set(page),
      error: (error: unknown) => this.error.set(mapApiError(error)),
    });
  }
}
