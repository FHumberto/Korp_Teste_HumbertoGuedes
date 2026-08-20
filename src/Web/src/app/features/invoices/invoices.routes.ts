import { Routes } from '@angular/router';

export const INVOICE_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./pages/invoice-list-page').then((page) => page.InvoiceListPage), title: 'Notas fiscais | Korp' },
  { path: 'new', loadComponent: () => import('./pages/invoice-create-page').then((page) => page.InvoiceCreatePage), title: 'Nova nota | Korp' },
  { path: ':id', loadComponent: () => import('./pages/invoice-details-page').then((page) => page.InvoiceDetailsPage), title: 'Detalhe da nota | Korp' },
];
