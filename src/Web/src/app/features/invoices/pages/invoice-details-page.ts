import { Component } from '@angular/core';
import { FeaturePlaceholderPage } from '../../../shared/pages/feature-placeholder-page';

@Component({
  imports: [FeaturePlaceholderPage],
  template: `<app-feature-placeholder-page eyebrow="Faturamento" title="Detalhe da nota" description="Consulte os itens, o estado e realize o fechamento da nota." />`,
})
export class InvoiceDetailsPage {}
