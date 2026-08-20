import { Component } from '@angular/core';
import { FeaturePlaceholderPage } from '../../../shared/pages/feature-placeholder-page';

@Component({
  imports: [FeaturePlaceholderPage],
  template: `<app-feature-placeholder-page eyebrow="Estoque" title="Produtos" description="Consulte os produtos cadastrados e seus saldos atuais." actionLabel="Novo produto" actionLink="/products/new" />`,
})
export class ProductListPage {}
