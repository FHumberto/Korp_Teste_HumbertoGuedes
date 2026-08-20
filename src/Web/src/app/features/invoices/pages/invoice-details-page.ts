import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { InvoiceDetails } from '../components/invoice-details';

@Component({
  imports: [InvoiceDetails, RouterLink],
  templateUrl: './invoice-details-page.html',
})
export class InvoiceDetailsPage {
  protected readonly invoiceId = inject(ActivatedRoute).snapshot.paramMap.get('id') ?? '';
}
