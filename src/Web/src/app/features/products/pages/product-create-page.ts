import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ProductCreateForm } from '../components/product-create-form';

@Component({
  imports: [ProductCreateForm, RouterLink],
  templateUrl: './product-create-page.html',
})
export class ProductCreatePage {
  private readonly router = inject(Router);
  protected onCreated(): void { void this.router.navigate(['/products'], { queryParams: { created: true } }); }
  protected onCancelled(): void { void this.router.navigate(['/products']); }
}
