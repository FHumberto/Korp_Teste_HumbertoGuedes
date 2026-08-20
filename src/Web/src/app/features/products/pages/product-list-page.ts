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
  templateUrl: './product-list-page.html',
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
