import { Routes } from '@angular/router';

export const PRODUCT_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./pages/product-list-page').then((page) => page.ProductListPage), title: 'Produtos | Korp' },
  { path: 'new', loadComponent: () => import('./pages/product-create-page').then((page) => page.ProductCreatePage), title: 'Novo produto | Korp' },
];
