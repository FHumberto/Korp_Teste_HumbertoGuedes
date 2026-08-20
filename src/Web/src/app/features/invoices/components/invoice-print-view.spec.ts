import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { InvoicePrintView } from './invoice-print-view';

@Component({
  imports: [InvoicePrintView],
  template: `<app-invoice-print-view [invoice]="invoice" />`,
})
class TestHost {
  readonly invoice = {
    id: 'invoice-1', number: 42, status: 'closed' as const, createdAt: '2026-08-20T10:00:00Z', closedAt: '2026-08-20T10:05:00Z',
    items: [{ productId: 'product-1', productCode: 'PROD-001', productDescription: 'Produto demonstrativo', quantity: 2 }],
  };
}

describe('InvoicePrintView', () => {
  it('should render the complete simplified document', async () => {
    const fixture = TestBed.createComponent(TestHost);
    fixture.detectChanges();
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('h1')?.textContent).toContain('NF: 000042');
    expect(element.querySelector('table')?.textContent).toContain('PROD-001');
    expect(element.querySelector('table')?.textContent).toContain('Produto demonstrativo');
    expect(element.querySelector('footer')?.textContent).toContain('sem validade fiscal');
  });
});
