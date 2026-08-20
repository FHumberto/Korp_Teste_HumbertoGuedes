import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'products' },
  {
    path: 'products',
    loadChildren: () => import('./features/products/products.routes').then((routes) => routes.PRODUCT_ROUTES),
  },
  {
    path: 'invoices',
    loadChildren: () => import('./features/invoices/invoices.routes').then((routes) => routes.INVOICE_ROUTES),
  },
  { path: '**', loadComponent: () => import('./shared/pages/not-found-page').then((page) => page.NotFoundPage) },
];
