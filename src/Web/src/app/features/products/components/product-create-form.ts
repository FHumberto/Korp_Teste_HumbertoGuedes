import { Component, DestroyRef, inject, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormField, form, maxLength, min, required, validate } from '@angular/forms/signals';
import { finalize } from 'rxjs';
import { mapApiError } from '../../../core/http/api-error.mapper';
import { ApiError } from '../../../core/http/problem-details';
import { FeedbackMessage } from '../../../shared/components/feedback-message';
import { ProcessingButton } from '../../../shared/components/processing-button';
import { ProductsApiService } from '../data-access/products-api.service';
import { CreateProductRequest, PRODUCT_LIMITS } from '../models/product.models';

@Component({
  selector: 'app-product-create-form',
  imports: [FeedbackMessage, FormField, ProcessingButton],
  template: `
    <form (submit)="onSubmit($event)" class="space-y-6" novalidate>
      @if (error(); as apiError) { <app-feedback-message kind="error" title="Não foi possível cadastrar o produto." [message]="apiError.message" [traceId]="apiError.traceId" /> }

      <div>
        <label for="code" class="block text-sm font-semibold text-slate-800">Código</label>
        <input id="code" type="text" [formField]="productForm.code" autocomplete="off" class="mt-2 block w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-950 uppercase shadow-sm focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-200" [attr.aria-describedby]="showCodeError() ? 'code-error' : 'code-help'" />
        @if (showCodeError()) { <p id="code-error" class="mt-2 text-sm text-red-700">{{ productForm.code().errors()[0].message }}</p> }
        @else { <p id="code-help" class="mt-2 text-xs text-slate-500">Até {{ limits.code }} caracteres. O código será salvo em maiúsculas.</p> }
      </div>

      <div>
        <label for="description" class="block text-sm font-semibold text-slate-800">Descrição</label>
        <input id="description" type="text" [formField]="productForm.description" autocomplete="off" class="mt-2 block w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-950 shadow-sm focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-200" [attr.aria-describedby]="showDescriptionError() ? 'description-error' : 'description-help'" />
        @if (showDescriptionError()) { <p id="description-error" class="mt-2 text-sm text-red-700">{{ productForm.description().errors()[0].message }}</p> }
        @else { <p id="description-help" class="mt-2 text-xs text-slate-500">Até {{ limits.description }} caracteres.</p> }
      </div>

      <div>
        <label for="initialBalance" class="block text-sm font-semibold text-slate-800">Saldo inicial</label>
        <input id="initialBalance" type="number" step="1" inputmode="numeric" [formField]="productForm.initialBalance" class="mt-2 block w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-950 shadow-sm focus:border-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-200" [attr.aria-describedby]="showBalanceError() ? 'balance-error' : 'balance-help'" />
        @if (showBalanceError()) { <p id="balance-error" class="mt-2 text-sm text-red-700">{{ productForm.initialBalance().errors()[0].message }}</p> }
        @else { <p id="balance-help" class="mt-2 text-xs text-slate-500">Informe uma quantidade inteira igual ou maior que zero.</p> }
      </div>

      <div class="flex flex-wrap justify-end gap-3 border-t border-slate-200 pt-5">
        <button type="button" (click)="cancel()" [disabled]="submitting()" class="inline-flex min-h-10 items-center rounded-lg border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50">Cancelar</button>
        <app-processing-button label="Cadastrar produto" processingLabel="Cadastrando..." [processing]="submitting()" />
      </div>
    </form>
  `,
})
export class ProductCreateForm {
  readonly created = output<void>();
  readonly cancelled = output<void>();

  private readonly api = inject(ProductsApiService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly limits = PRODUCT_LIMITS;
  protected readonly submitting = signal(false);
  protected readonly error = signal<ApiError | null>(null);
  protected readonly productModel = signal<CreateProductRequest>({ code: '', description: '', initialBalance: 0 });
  protected readonly productForm = form(this.productModel, (product) => {
    required(product.code, { message: 'O código do produto é obrigatório.' });
    maxLength(product.code, PRODUCT_LIMITS.code, { message: `O código deve possuir no máximo ${PRODUCT_LIMITS.code} caracteres.` });
    required(product.description, { message: 'A descrição do produto é obrigatória.' });
    maxLength(product.description, PRODUCT_LIMITS.description, { message: `A descrição deve possuir no máximo ${PRODUCT_LIMITS.description} caracteres.` });
    min(product.initialBalance, 0, { message: 'O saldo inicial não pode ser negativo.' });
    validate(product.initialBalance, ({ value }) => Number.isInteger(value()) ? undefined : { kind: 'integer', message: 'O saldo inicial deve ser um número inteiro.' });
  });

  protected showCodeError(): boolean { return this.productForm.code().touched() && this.productForm.code().invalid(); }
  protected showDescriptionError(): boolean { return this.productForm.description().touched() && this.productForm.description().invalid(); }
  protected showBalanceError(): boolean { return this.productForm.initialBalance().touched() && this.productForm.initialBalance().invalid(); }
  protected cancel(): void { if (!this.submitting()) this.cancelled.emit(); }

  protected onSubmit(event: SubmitEvent): void {
    event.preventDefault();
    this.productForm().markAsTouched();
    if (this.productForm().invalid() || this.submitting()) return;

    const value = this.productModel();
    const request: CreateProductRequest = { code: value.code.trim(), description: value.description.trim(), initialBalance: value.initialBalance };
    this.error.set(null);
    this.submitting.set(true);
    this.api.create(request).pipe(finalize(() => this.submitting.set(false)), takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.created.emit(),
      error: (error: unknown) => this.error.set(mapApiError(error)),
    });
  }
}
