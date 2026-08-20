import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { Invoice } from '../models/invoice.models';
import { InvoiceNumberPipe } from './invoice-number.pipe';

@Component({
  selector: 'app-invoice-print-view',
  imports: [DatePipe, InvoiceNumberPipe],
  host: { class: 'block' },
  templateUrl: './invoice-print-view.html',
})
export class InvoicePrintView {
  readonly invoice = input.required<Invoice>();
}
