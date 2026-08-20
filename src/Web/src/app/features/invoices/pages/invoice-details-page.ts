import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { mapApiError } from '../../../core/http/api-error.mapper';
import { ApiError } from '../../../core/http/problem-details';
import { FeedbackMessage } from '../../../shared/components/feedback-message';
import { LoadingIndicator } from '../../../shared/components/loading-indicator';
import { ProcessingButton } from '../../../shared/components/processing-button';
import { InvoicePrintView } from '../components/invoice-print-view';
import { InvoicesApiService } from '../data-access/invoices-api.service';
import { Invoice } from '../models/invoice.models';

@Component({
  imports: [FeedbackMessage, InvoicePrintView, LoadingIndicator, ProcessingButton, RouterLink],
  template: `
    <section>
      <div class="print:hidden"><a routerLink="/invoices" class="text-sm font-semibold text-blue-700 hover:text-blue-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-700">← Voltar para notas fiscais</a></div>

      @if (loading()) {
        <div class="mt-6 rounded-xl border border-slate-200 bg-white p-6"><app-loading-indicator label="Carregando nota fiscal..." /></div>
      } @else if (loadError(); as apiError) {
        <div class="mt-6 space-y-4"><app-feedback-message kind="error" title="Não foi possível carregar a nota." [message]="apiError.message" [traceId]="apiError.traceId" /><button type="button" (click)="loadInvoice()" class="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-semibold hover:bg-slate-50">Tentar novamente</button></div>
      } @else if (invoice(); as currentInvoice) {
        <div class="mt-5">
          <app-invoice-print-view [invoice]="currentInvoice" />

          <div class="mt-6 space-y-4 print:hidden" aria-live="polite">
            @if (closeError(); as apiError) { <app-feedback-message kind="error" title="O fechamento não foi concluído." [message]="apiError.message" [traceId]="apiError.traceId" /> }
            @if (closedSuccessfully()) { <app-feedback-message kind="success" title="Nota fechada com sucesso." message="A baixa do estoque foi confirmada e a impressão foi liberada." /> }
          </div>

          @if (currentInvoice.status === 'open') {
            <div class="mt-6 flex justify-end print:hidden"><app-processing-button (click)="closeAndPrint()" label="Imprimir" processingLabel="Processando fechamento..." [processing]="closing()" /></div>
          }
        </div>
      }
    </section>
  `,
})
export class InvoiceDetailsPage {
  private readonly api = inject(InvoicesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly invoiceId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly invoice = signal<Invoice | null>(null);
  protected readonly loading = signal(true);
  protected readonly closing = signal(false);
  protected readonly loadError = signal<ApiError | null>(null);
  protected readonly closeError = signal<ApiError | null>(null);
  protected readonly closedSuccessfully = signal(false);

  constructor() { this.loadInvoice(); }

  protected loadInvoice(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.getById(this.invoiceId).pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (invoice) => this.invoice.set(invoice),
      error: (error: unknown) => this.loadError.set(mapApiError(error)),
    });
  }

  protected closeAndPrint(): void {
    const currentInvoice = this.invoice();
    if (!currentInvoice || currentInvoice.status !== 'open' || this.closing()) return;

    this.closing.set(true);
    this.closeError.set(null);
    this.closedSuccessfully.set(false);
    this.api.close(currentInvoice.id).pipe(finalize(() => this.closing.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (closed) => {
        this.invoice.set({ ...currentInvoice, status: closed.status, closedAt: closed.closedAt });
        this.closedSuccessfully.set(true);
        globalThis.setTimeout(() => globalThis.print(), 0);
      },
      error: (error: unknown) => {
        const apiError = mapApiError(error);
        this.closeError.set(apiError);
        if (apiError.code === 'INVOICE_ALREADY_CLOSED') this.loadInvoice();
      },
    });
  }
}
