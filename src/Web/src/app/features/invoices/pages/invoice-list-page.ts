import { Component } from '@angular/core';
import { FeaturePlaceholderPage } from '../../../shared/pages/feature-placeholder-page';

@Component({
  imports: [FeaturePlaceholderPage],
  template: `<app-feature-placeholder-page eyebrow="Faturamento" title="Notas fiscais" description="Consulte notas abertas e fechadas." actionLabel="Nova nota" actionLink="/invoices/new" />`,
})
export class InvoiceListPage {}
