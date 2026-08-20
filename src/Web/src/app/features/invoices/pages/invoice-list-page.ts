import { DatePipe } from '@angular/common';
import { Component, DestroyRef, ElementRef, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { mapApiError } from '../../../core/http/api-error.mapper';
import { ApiError } from '../../../core/http/problem-details';
import { EmptyState } from '../../../shared/components/empty-state';
import { FeedbackMessage } from '../../../shared/components/feedback-message';
import { LoadingIndicator } from '../../../shared/components/loading-indicator';
import { InvoicesApiService } from '../data-access/invoices-api.service';
import { Invoice, InvoiceStatus, InvoiceSummary } from '../models/invoice.models';
import { InvoiceNumberPipe } from '../components/invoice-number.pipe';
import { InvoiceDetails } from '../components/invoice-details';
import { InvoiceCreateForm } from './invoice-create-page';

type StatusFilter = InvoiceStatus | 'all';

@Component({
  imports: [DatePipe, EmptyState, FeedbackMessage, InvoiceCreateForm, InvoiceDetails, InvoiceNumberPipe, LoadingIndicator],
  template: `
    <section>
      <div class="flex flex-wrap items-start justify-between gap-4">
        <div><p class="text-sm font-semibold text-blue-700">Faturamento</p><h1 class="mt-1 text-3xl font-bold tracking-tight">Notas fiscais</h1><p class="mt-2 text-slate-600">Consulte notas abertas e fechadas.</p></div>
        <button type="button" (click)="openCreateDialog()" class="rounded-lg bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-700">Nova nota</button>
      </div>

      <div class="mt-6 flex items-end gap-3">
        <div><label for="status-filter" class="block text-sm font-semibold text-slate-800">Status</label><select id="status-filter" [value]="filter()" (change)="changeFilter($event)" class="mt-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-200"><option value="all">Todas</option><option value="open">Abertas</option><option value="closed">Fechadas</option></select></div>
      </div>

      <div class="mt-6 space-y-4">
        @if (error(); as apiError) { <app-feedback-message kind="error" title="Não foi possível carregar as notas." [message]="apiError.message" [traceId]="apiError.traceId" /> }
        @if (loading()) { <div class="rounded-xl border border-slate-200 bg-white p-6"><app-loading-indicator label="Carregando notas fiscais..." /></div> }
        @else if (invoices().length === 0) { <app-empty-state title="Nenhuma nota encontrada" description="Crie uma nota ou altere o filtro selecionado." /> }
        @else {
          <div class="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm"><div class="overflow-x-auto"><table class="w-full border-collapse text-left text-sm"><caption class="sr-only">Notas fiscais cadastradas</caption><thead class="bg-slate-100 text-slate-700"><tr><th scope="col" class="px-4 py-3 font-semibold">Número</th><th scope="col" class="px-4 py-3 font-semibold">Status</th><th scope="col" class="px-4 py-3 text-right font-semibold">Itens</th><th scope="col" class="px-4 py-3 font-semibold">Criação</th><th scope="col" class="px-4 py-3"><span class="sr-only">Ações</span></th></tr></thead><tbody class="divide-y divide-slate-200">
            @for (invoice of invoices(); track invoice.id) { <tr><td class="px-4 py-3 font-semibold tabular-nums">{{ invoice.number | invoiceNumber }}</td><td class="px-4 py-3"><span class="inline-flex rounded-full px-2.5 py-1 text-xs font-semibold" [class]="invoice.status === 'open' ? 'bg-amber-100 text-amber-900' : 'bg-emerald-100 text-emerald-900'">{{ invoice.status === 'open' ? 'Aberta' : 'Fechada' }}</span></td><td class="px-4 py-3 text-right tabular-nums">{{ invoice.itemCount }}</td><td class="whitespace-nowrap px-4 py-3 text-slate-700">{{ invoice.createdAt | date:'dd/MM/yyyy HH:mm' }}</td><td class="px-4 py-3 text-right"><button type="button" (click)="openDetailsDialog(invoice.id)" class="font-semibold text-blue-700 hover:text-blue-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-700">Ver detalhes</button></td></tr> }
          </tbody></table></div></div>
        }
      </div>

      <dialog #createDialog (cancel)="closeCreateDialog($event)" aria-labelledby="create-invoice-title" class="m-auto w-[calc(100%-2rem)] max-w-4xl rounded-xl bg-white p-0 text-slate-950 shadow-2xl backdrop:bg-slate-950/55">
        <div class="max-h-[calc(100vh-2rem)] overflow-y-auto p-6 sm:p-8">
          <div class="mb-6 flex items-start justify-between gap-4">
            <div><p class="text-sm font-semibold text-blue-700">Faturamento</p><h2 id="create-invoice-title" class="mt-1 text-2xl font-bold tracking-tight">Nova nota fiscal</h2><p class="mt-2 text-sm text-slate-600">Adicione os produtos e as quantidades.</p></div>
            <button type="button" (click)="closeCreateDialog()" class="rounded-md p-2 text-2xl leading-none text-slate-500 hover:bg-slate-100 hover:text-slate-800" aria-label="Fechar cadastro de nota fiscal">×</button>
          </div>
          @if (createDialogOpen()) {
            <app-invoice-create-form (created)="onInvoiceCreated()" (cancelled)="closeCreateDialog()" />
          }
        </div>
      </dialog>

      <dialog #detailsDialog (cancel)="closeDetailsDialog($event)" (click)="closeDetailsOnBackdrop($event)" aria-labelledby="invoice-details-title" class="m-auto w-[calc(100%-2rem)] max-w-5xl rounded-xl bg-white p-0 text-slate-950 shadow-2xl backdrop:bg-slate-950/55">
        <div class="max-h-[calc(100vh-2rem)] overflow-y-auto p-6 sm:p-8">
          <div class="mb-6 flex items-start justify-between gap-4 print:hidden">
            <div><p class="text-sm font-semibold text-blue-700">Faturamento</p><h2 id="invoice-details-title" class="mt-1 text-2xl font-bold tracking-tight">Detalhes da nota fiscal</h2></div>
            <button type="button" (click)="closeDetailsDialog()" class="rounded-md p-2 text-2xl leading-none text-slate-500 hover:bg-slate-100 hover:text-slate-800" aria-label="Fechar detalhes da nota fiscal">×</button>
          </div>
          @if (selectedInvoiceId(); as invoiceId) {
            <app-invoice-details [invoiceId]="invoiceId" (invoiceClosed)="onInvoiceClosed($event)" />
          }
        </div>
      </dialog>
    </section>
  `,
})
export class InvoiceListPage {
  private readonly api = inject(InvoicesApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly createDialog = viewChild.required<ElementRef<HTMLDialogElement>>('createDialog');
  private readonly detailsDialog = viewChild.required<ElementRef<HTMLDialogElement>>('detailsDialog');
  protected readonly invoices = signal<readonly InvoiceSummary[]>([]);
  protected readonly filter = signal<StatusFilter>('all');
  protected readonly loading = signal(true);
  protected readonly error = signal<ApiError | null>(null);
  protected readonly createDialogOpen = signal(false);
  protected readonly selectedInvoiceId = signal<string | null>(null);

  constructor() { this.load(); }

  protected changeFilter(event: Event): void {
    this.filter.set((event.target as HTMLSelectElement).value as StatusFilter);
    this.load();
  }

  protected openCreateDialog(): void { this.createDialogOpen.set(true); this.createDialog().nativeElement.showModal(); }
  protected closeCreateDialog(event?: Event): void { event?.preventDefault(); this.createDialog().nativeElement.close(); this.createDialogOpen.set(false); }
  protected onInvoiceCreated(): void { this.closeCreateDialog(); this.load(); }
  protected openDetailsDialog(invoiceId: string): void { this.selectedInvoiceId.set(invoiceId); this.detailsDialog().nativeElement.showModal(); }
  protected closeDetailsDialog(event?: Event): void { event?.preventDefault(); this.detailsDialog().nativeElement.close(); this.selectedInvoiceId.set(null); }
  protected closeDetailsOnBackdrop(event: MouseEvent): void { if (event.target === this.detailsDialog().nativeElement) this.closeDetailsDialog(); }
  protected onInvoiceClosed(invoice: Invoice): void {
    this.invoices.update((invoices) => invoices.map((summary) => summary.id === invoice.id ? { ...summary, status: invoice.status, closedAt: invoice.closedAt } : summary));
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    const filter = this.filter();
    const status: InvoiceStatus | undefined = filter === 'all' ? undefined : filter;
    this.api.list(status).pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (invoices) => this.invoices.set(invoices),
      error: (error: unknown) => this.error.set(mapApiError(error)),
    });
  }
}
