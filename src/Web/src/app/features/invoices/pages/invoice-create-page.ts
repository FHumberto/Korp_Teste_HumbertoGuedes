import { Component } from '@angular/core';
import { FeaturePlaceholderPage } from '../../../shared/pages/feature-placeholder-page';

@Component({
  imports: [FeaturePlaceholderPage],
  template: `<app-feature-placeholder-page eyebrow="Faturamento" title="Nova nota fiscal" description="Adicione produtos e quantidades para criar uma nota aberta." />`,
})
export class InvoiceCreatePage {}
