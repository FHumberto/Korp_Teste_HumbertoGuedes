import { Component, DestroyRef, effect, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { mapApiError } from '../../../core/http/api-error.mapper';
import { ApiError } from '../../../core/http/problem-details';
import { FeedbackMessage } from '../../../shared/components/feedback-message';
import { LoadingIndicator } from '../../../shared/components/loading-indicator';
import { ProcessingButton } from '../../../shared/components/processing-button';
import { InvoicesApiService } from '../data-access/invoices-api.service';
import { Invoice } from '../models/invoice.models';
import { InvoicePrintView } from './invoice-print-view';

@Component({
  selector: 'app-invoice-details',
  imports: [FeedbackMessage, InvoicePrintView, LoadingIndicator, ProcessingButton],
  template: `
    @if (loading()) {
      <div class="rounded-xl border border-slate-200 bg-white p-6"><app-loading-indicator label="Carregando nota fiscal..." /></div>
    } @else if (loadError(); as apiError) {
      <div class="space-y-4"><app-feedback-message kind="error" title="Não foi possível carregar a nota." [message]="apiError.message" [traceId]="apiError.traceId" /><button type="button" (click)="loadInvoice()" class="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-semibold hover:bg-slate-50">Tentar novamente</button></div>
    } @else if (invoice(); as currentInvoice) {
      <app-invoice-print-view [invoice]="currentInvoice" />
      <div class="mt-6 space-y-4 print:hidden" aria-live="polite">
        @if (closeError(); as apiError) { <app-feedback-message kind="error" title="O fechamento não foi concluído." [message]="apiError.message" [traceId]="apiError.traceId" /> }
        @if (documentError(); as apiError) { <app-feedback-message kind="error" title="A nota foi fechada, mas o PDF não pôde ser aberto." [message]="apiError.message" [traceId]="apiError.traceId" /> }
        @if (closedSuccessfully()) { <app-feedback-message kind="success" title="Nota fechada com sucesso." message="A baixa do estoque foi confirmada." /> }
      </div>
      @if (currentInvoice.status === 'open') {
        <div class="mt-6 flex justify-end print:hidden"><app-processing-button (click)="closeAndPrint()" label="Imprimir e fechar" processingLabel="Processando fechamento..." [processing]="closing()" /></div>
      } @else {
        <div class="mt-6 flex justify-end print:hidden"><app-processing-button (click)="openDocument()" label="Visualizar PDF" processingLabel="Carregando PDF..." [processing]="loadingDocument()" /></div>
      }
    }
  `,
})
export class InvoiceDetails {
  readonly invoiceId = input.required<string>();
  readonly invoiceClosed = output<Invoice>();
  private readonly api = inject(InvoicesApiService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly invoice = signal<Invoice | null>(null);
  protected readonly loading = signal(true);
  protected readonly closing = signal(false);
  protected readonly loadError = signal<ApiError | null>(null);
  protected readonly closeError = signal<ApiError | null>(null);
  protected readonly documentError = signal<ApiError | null>(null);
  protected readonly closedSuccessfully = signal(false);
  protected readonly loadingDocument = signal(false);

  constructor() { effect(() => { this.invoiceId(); this.loadInvoice(); }); }

  protected loadInvoice(): void {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.getById(this.invoiceId()).pipe(finalize(() => this.loading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
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
        const updatedInvoice: Invoice = { ...currentInvoice, status: closed.status, closedAt: closed.closedAt };
        this.invoice.set(updatedInvoice);
        this.invoiceClosed.emit(updatedInvoice);
        this.closedSuccessfully.set(true);
        this.openDocument();
      },
      error: (error: unknown) => {
        const apiError = mapApiError(error);
        this.closeError.set(apiError);
        if (apiError.code === 'INVOICE_ALREADY_CLOSED') this.loadInvoice();
      },
    });
  }

  protected openDocument(): void {
    const currentInvoice = this.invoice();
    if (!currentInvoice || currentInvoice.status !== 'closed' || this.loadingDocument()) return;
    this.loadingDocument.set(true);
    this.documentError.set(null);
    this.api.getDocument(currentInvoice.id).pipe(finalize(() => this.loadingDocument.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (pdf) => {
        const url = URL.createObjectURL(pdf);
        const link = document.createElement('a');
        link.href = url;
        link.target = '_blank';
        link.rel = 'noopener';
        link.click();
        globalThis.setTimeout(() => URL.revokeObjectURL(url), 60_000);
      },
      error: (error: unknown) => this.documentError.set(mapApiError(error)),
    });
  }
}
