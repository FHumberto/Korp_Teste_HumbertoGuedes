import { Component } from '@angular/core';
import { FeaturePlaceholderPage } from '../../../shared/pages/feature-placeholder-page';

@Component({
  imports: [FeaturePlaceholderPage],
  template: `<app-feature-placeholder-page eyebrow="Estoque" title="Novo produto" description="Cadastre código, descrição e saldo inicial do produto." />`,
})
export class ProductCreatePage {}
