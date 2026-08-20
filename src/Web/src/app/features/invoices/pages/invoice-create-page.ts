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
  template: `
        @if (productsLoading()) { <div class="rounded-xl border border-slate-200 bg-white p-6"><app-loading-indicator label="Carregando produtos..." /></div> }
        @else if (productsError(); as apiError) { <div class="space-y-3"><app-feedback-message kind="error" title="Não foi possível carregar os produtos." [message]="apiError.message" [traceId]="apiError.traceId" /><button type="button" (click)="loadProducts()" class="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-semibold hover:bg-slate-50">Tentar novamente</button></div> }
        @else if (products().length === 0) { <div class="space-y-4"><app-empty-state title="Nenhum produto disponível" description="Cadastre ao menos um produto antes de criar a nota." /><a routerLink="/products/new" class="inline-flex rounded-lg bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-800">Cadastrar produto</a></div> }
        @else {
          <form (submit)="onSubmit($event)" class="space-y-6" novalidate>
            @if (submitError(); as apiError) { <app-feedback-message kind="error" title="Não foi possível criar a nota." [message]="apiError.message" [traceId]="apiError.traceId" /> }
            @if (showItemsError()) { <app-feedback-message kind="error" title="Revise os itens da nota." [message]="invoiceForm.items().errors()[0].message ?? 'Os itens informados são inválidos.'" /> }

            <div class="space-y-4">
              @for (item of draft().items; track $index; let itemIndex = $index) {
                <fieldset class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
                  <legend class="px-1 text-sm font-bold text-slate-800">Item {{ itemIndex + 1 }}</legend>
                  <div class="grid gap-4 md:grid-cols-[1fr_10rem_auto] md:items-start">
                    <div>
                      <label [for]="'product-' + itemIndex" class="block text-sm font-semibold text-slate-800">Produto</label>
                      <select [id]="'product-' + itemIndex" [formField]="invoiceForm.items[itemIndex].productId" [attr.aria-describedby]="'product-help-' + itemIndex" class="mt-2 block w-full rounded-lg border border-slate-300 bg-white px-3 py-2 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-200">
                        <option value="">Selecione um produto</option>
                        @for (product of products(); track product.id) { <option [value]="product.id" [disabled]="isSelectedByAnotherItem(product.id, itemIndex)">{{ product.code }} — {{ product.description }}</option> }
                      </select>
                      @if (invoiceForm.items[itemIndex].productId().touched() && invoiceForm.items[itemIndex].productId().invalid()) { <p [id]="'product-help-' + itemIndex" class="mt-2 text-sm text-red-700">{{ invoiceForm.items[itemIndex].productId().errors()[0].message }}</p> }
                      @else if (selectedProduct(item.productId); as product) { <p [id]="'product-help-' + itemIndex" class="mt-2 text-xs text-slate-500">Saldo atual: <strong>{{ product.balance }}</strong>. A validação definitiva ocorrerá no fechamento.</p> }
                      @else { <p [id]="'product-help-' + itemIndex" class="sr-only">Selecione um produto para este item.</p> }
                    </div>
                    <div>
                      <label [for]="'quantity-' + itemIndex" class="block text-sm font-semibold text-slate-800">Quantidade</label>
                      <input [id]="'quantity-' + itemIndex" type="number" step="1" inputmode="numeric" [formField]="invoiceForm.items[itemIndex].quantity" [attr.aria-describedby]="'quantity-help-' + itemIndex" class="mt-2 block w-full rounded-lg border border-slate-300 px-3 py-2 focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-200" />
                      @if (invoiceForm.items[itemIndex].quantity().touched() && invoiceForm.items[itemIndex].quantity().invalid()) { <p [id]="'quantity-help-' + itemIndex" class="mt-2 text-sm text-red-700">{{ invoiceForm.items[itemIndex].quantity().errors()[0].message }}</p> }
                      @else { <p [id]="'quantity-help-' + itemIndex" class="sr-only">Informe uma quantidade inteira maior que zero.</p> }
                    </div>
                    <button type="button" (click)="removeItem(itemIndex)" [disabled]="draft().items.length === 1" class="mt-7 min-h-10 rounded-lg border border-red-300 px-3 py-2 text-sm font-semibold text-red-800 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-40" [attr.aria-label]="'Remover item ' + (itemIndex + 1)">Remover</button>
                  </div>
                </fieldset>
              }
            </div>

            <button type="button" (click)="addItem()" class="rounded-lg border border-blue-300 bg-white px-4 py-2 text-sm font-semibold text-blue-800 hover:bg-blue-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-700">+ Adicionar produto</button>
            <div class="flex flex-wrap justify-end gap-3 border-t border-slate-200 pt-5"><button type="button" (click)="cancel()" [disabled]="submitting()" class="inline-flex min-h-10 items-center rounded-lg border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50">Cancelar</button><app-processing-button label="Criar nota" processingLabel="Criando nota..." [processing]="submitting()" /></div>
          </form>
        }
  `,
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
    this.productsApi.listAll().pipe(finalize(() => this.productsLoading.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
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
  template: `
    <section class="mx-auto max-w-4xl">
      <a routerLink="/invoices" class="text-sm font-semibold text-blue-700 hover:text-blue-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-700">← Voltar para notas fiscais</a>
      <div class="mt-5"><p class="text-sm font-semibold text-blue-700">Faturamento</p><h1 class="mt-1 text-3xl font-bold tracking-tight">Nova nota fiscal</h1><p class="mt-2 text-slate-600">Adicione os produtos e as quantidades.</p></div>
      <div class="mt-8"><app-invoice-create-form (created)="onCreated($event)" (cancelled)="onCancelled()" /></div>
    </section>
  `,
})
export class InvoiceCreatePage {
  private readonly router = inject(Router);
  protected onCreated(invoice: Invoice): void { void this.router.navigate(['/invoices', invoice.id]); }
  protected onCancelled(): void { void this.router.navigate(['/invoices']); }
}
