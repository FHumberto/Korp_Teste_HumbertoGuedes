import { Component, DestroyRef, inject, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormField, applyEach, form, min, required, schema, validate } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { mapApiError } from '../../../core/http/api-error.mapper';
import { ApiError } from '../../../core/http/problem-details';
import { ProductsApiService } from '../../products/data-access/products-api.service';
import { ProductSummary } from '../../products/models/product.models';
import { EmptyState } from '../../../shared/components/empty-state';
import { FeedbackMessage } from '../../../shared/components/feedback-message';
import { LoadingIndicator } from '../../../shared/components/loading-indicator';
import { ProcessingButton } from '../../../shared/components/processing-button';
import { InvoicesApiService } from '../data-access/invoices-api.service';
import { CreateInvoiceRequest, Invoice } from '../models/invoice.models';

interface InvoiceDraftItem { productId: string; quantity: number; }
interface InvoiceDraft { items: InvoiceDraftItem[]; }

const itemSchema = schema<InvoiceDraftItem>((item) => {
  required(item.productId, { message: 'Selecione um produto.' });
  min(item.quantity, 1, { message: 'A quantidade deve ser maior que zero.' });
  validate(item.quantity, ({ value }) => Number.isInteger(value()) ? undefined : { kind: 'integer', message: 'A quantidade deve ser um número inteiro.' });
});

@Component({
  selector: 'app-invoice-create-form',
  imports: [EmptyState, FeedbackMessage, FormField, LoadingIndicator, ProcessingButton, RouterLink],
  templateUrl: './invoice-create-form.html',
})
export class InvoiceCreateForm {
  readonly created = output<Invoice>();
  readonly cancelled = output<void>();

  private readonly productsApi = inject(ProductsApiService);
  private readonly invoicesApi = inject(InvoicesApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly products = signal<readonly ProductSummary[]>([]);
  protected readonly productsLoading = signal(true);
  protected readonly productsError = signal<ApiError | null>(null);
  protected readonly submitError = signal<ApiError | null>(null);
  protected readonly submitting = signal(false);
  protected readonly draft = signal<InvoiceDraft>({ items: [{ productId: '', quantity: 1 }] });
  protected readonly invoiceForm = form(this.draft, (invoice) => {
    applyEach(invoice.items, itemSchema);
    validate(invoice.items, ({ value }) => {
      const selectedIds = value().map((item) => item.productId).filter(Boolean);
      return new Set(selectedIds).size === selectedIds.length ? undefined : { kind: 'duplicate', message: 'O mesmo produto não pode ser adicionado mais de uma vez.' };
    });
  });

  constructor() { this.loadProducts(); }

  protected loadProducts(): void {
    this.productsLoading.set(true);
    this.productsError.set(null);
    this.productsApi.listAvailable().pipe(finalize(() => this.productsLoading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (products) => this.products.set(products),
      error: (error: unknown) => this.productsError.set(mapApiError(error)),
    });
  }

  protected addItem(): void { this.draft.update((draft) => ({ items: [...draft.items, { productId: '', quantity: 1 }] })); }
  protected removeItem(index: number): void { this.draft.update((draft) => draft.items.length === 1 ? draft : ({ items: draft.items.filter((_, itemIndex) => itemIndex !== index) })); }
  protected selectedProduct(productId: string): ProductSummary | undefined { return this.products().find((product) => product.id === productId); }
  protected isSelectedByAnotherItem(productId: string, currentIndex: number): boolean { return this.draft().items.some((item, index) => index !== currentIndex && item.productId === productId); }
  protected showItemsError(): boolean { return this.invoiceForm.items().touched() && this.invoiceForm.items().errors().length > 0; }
  protected cancel(): void { if (!this.submitting()) this.cancelled.emit(); }

  protected onSubmit(event: SubmitEvent): void {
    event.preventDefault();
    this.invoiceForm().markAsTouched();
    if (this.invoiceForm().invalid() || this.submitting()) return;
    const request: CreateInvoiceRequest = { items: this.draft().items.map(({ productId, quantity }) => ({ productId, quantity })) };
    this.submitError.set(null);
    this.submitting.set(true);
    this.invoicesApi.create(request).pipe(finalize(() => this.submitting.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (invoice) => this.created.emit(invoice),
      error: (error: unknown) => this.submitError.set(mapApiError(error)),
    });
  }
}

@Component({
  imports: [InvoiceCreateForm, RouterLink],
  templateUrl: './invoice-create-page.html',
})
export class InvoiceCreatePage {
  private readonly router = inject(Router);
  protected onCreated(invoice: Invoice): void { void this.router.navigate(['/invoices', invoice.id]); }
  protected onCancelled(): void { void this.router.navigate(['/invoices']); }
}
