import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'invoiceNumber' })
export class InvoiceNumberPipe implements PipeTransform {
  transform(number: number): string {
    return `NF: ${number.toString().padStart(6, '0')}`;
  }
}
