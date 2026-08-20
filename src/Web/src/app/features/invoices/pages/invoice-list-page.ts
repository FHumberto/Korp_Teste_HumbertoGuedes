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
  templateUrl: './invoice-list-page.html',
})
export class InvoiceListPage {
  private readonly api = inject(InvoicesApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly createDialog = viewChild.required<ElementRef<HTMLDialogElement>>('createDialog');
  private readonly detailsDialog = viewChild.required<ElementRef<HTMLDialogElement>>('detailsDialog');
  private createDialogOpener: HTMLElement | null = null;
  private detailsDialogOpener: HTMLElement | null = null;
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

  protected clearFilter(): void { this.filter.set('all'); this.load(); }

  protected openCreateDialog(): void { this.createDialogOpener = document.activeElement as HTMLElement | null; this.createDialogOpen.set(true); const dialog = this.createDialog().nativeElement; dialog.showModal(); queueMicrotask(() => dialog.querySelector<HTMLElement>('select, input, button')?.focus()); }
  protected closeCreateDialog(event?: Event): void { event?.preventDefault(); this.createDialog().nativeElement.close(); this.createDialogOpen.set(false); this.createDialogOpener?.focus(); this.createDialogOpener = null; }
  protected onInvoiceCreated(): void { this.closeCreateDialog(); this.load(); }
  protected openDetailsDialog(invoiceId: string): void { this.detailsDialogOpener = document.activeElement as HTMLElement | null; this.selectedInvoiceId.set(invoiceId); const dialog = this.detailsDialog().nativeElement; dialog.showModal(); queueMicrotask(() => dialog.querySelector<HTMLElement>('button')?.focus()); }
  protected openDetailsFromKeyboard(event: KeyboardEvent, invoiceId: string): void {
    if (event.key !== 'Enter' && event.key !== ' ') return;
    event.preventDefault();
    this.openDetailsDialog(invoiceId);
  }
  protected closeDetailsDialog(event?: Event): void { event?.preventDefault(); this.detailsDialog().nativeElement.close(); this.selectedInvoiceId.set(null); this.detailsDialogOpener?.focus(); this.detailsDialogOpener = null; }
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
