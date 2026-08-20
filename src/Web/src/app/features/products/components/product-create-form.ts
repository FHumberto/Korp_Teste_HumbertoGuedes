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
  templateUrl: './product-create-form.html',
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
