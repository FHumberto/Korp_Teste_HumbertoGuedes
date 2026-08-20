import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { Invoice } from '../models/invoice.models';
import { InvoiceListPage } from './invoice-list-page';

describe('InvoiceListPage', () => {
  it('should reload invoices when status changes', async () => {
    TestBed.configureTestingModule({ imports: [InvoiceListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: '', billing: 'http://billing/api/v1' } }] });
    const fixture = TestBed.createComponent(InvoiceListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((request) => request.url === 'http://billing/api/v1/invoices' && !request.params.has('status')).flush([]);
    await fixture.whenStable();
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('#status-filter') as HTMLSelectElement;
    select.value = 'open';
    select.dispatchEvent(new Event('change'));
    http.expectOne((request) => request.params.get('status') === 'open').flush([]);
    http.verify();
  });

  it('should display the invoice number with the NF prefix and leading zeros', async () => {
    TestBed.configureTestingModule({ imports: [InvoiceListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: '', billing: 'http://billing/api/v1' } }] });
    const fixture = TestBed.createComponent(InvoiceListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((request) => request.url === 'http://billing/api/v1/invoices').flush([{ id: 'invoice-1', number: 42, status: 'open', itemCount: 1, createdAt: '2026-08-20T10:00:00Z', closedAt: null }]);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('NF: 000042');
    http.verify();
  });

  it('should open the invoice form in a dialog', () => {
    TestBed.configureTestingModule({ imports: [InvoiceListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: 'http://inventory/api/v1', billing: 'http://billing/api/v1' } }] });
    const fixture = TestBed.createComponent(InvoiceListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((request) => request.url === 'http://billing/api/v1/invoices').flush([]);
    fixture.detectChanges();

    const createButton = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    expect(createButton.textContent).toContain('Nova nota fiscal');
    expect(createButton.querySelector('svg')?.getAttribute('aria-hidden')).toBe('true');
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    dialog.showModal = () => dialog.setAttribute('open', '');
    dialog.close = () => dialog.removeAttribute('open');
    createButton.click();
    fixture.detectChanges();

    expect(dialog.open).toBe(true);
    expect(dialog.textContent).toContain('Nova nota fiscal');
    http.expectOne('http://inventory/api/v1/products/available').flush([{ id: 'product-1', code: 'PROD-001', description: 'Produto', balance: 10 }]);
    fixture.detectChanges();
    expect(dialog.querySelector('form')).not.toBeNull();
    http.verify();
  });

  it('should open invoice details by clicking the row and keep the explicit action', async () => {
    TestBed.configureTestingModule({ imports: [InvoiceListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: '', billing: 'http://billing/api/v1' } }] });
    const fixture = TestBed.createComponent(InvoiceListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('http://billing/api/v1/invoices').flush([{ id: 'invoice-1', number: 42, status: 'closed', itemCount: 1, createdAt: '2026-08-20T10:00:00Z', closedAt: '2026-08-20T10:05:00Z' }]);
    await fixture.whenStable();
    fixture.detectChanges();

    const dialog = fixture.nativeElement.querySelectorAll('dialog')[1] as HTMLDialogElement;
    dialog.showModal = () => dialog.setAttribute('open', '');
    dialog.close = () => dialog.removeAttribute('open');
    const detailsButton = Array.from(fixture.nativeElement.querySelectorAll('button')).find((button) => (button as HTMLButtonElement).textContent?.includes('Ver detalhes')) as HTMLButtonElement;
    expect(detailsButton).toBeTruthy();
    (fixture.nativeElement.querySelector('tbody tr') as HTMLTableRowElement).click();
    fixture.detectChanges();
    http.expectOne('http://billing/api/v1/invoices/invoice-1').flush({ id: 'invoice-1', number: 42, status: 'closed', items: [{ productId: 'product-1', productCode: 'PROD-001', productDescription: 'Produto', quantity: 2 }], createdAt: '2026-08-20T10:00:00Z', closedAt: '2026-08-20T10:05:00Z' });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(dialog.open).toBe(true);
    expect(dialog.textContent).toContain('Detalhes da nota fiscal');
    expect(dialog.textContent).toContain('PROD-001');
    http.verify();
  });

  it('should open invoice details from the keyboard', async () => {
    TestBed.configureTestingModule({ imports: [InvoiceListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: '', billing: 'http://billing/api/v1' } }] });
    const fixture = TestBed.createComponent(InvoiceListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('http://billing/api/v1/invoices').flush([{ id: 'invoice-1', number: 42, status: 'closed', itemCount: 1, createdAt: '2026-08-20T10:00:00Z', closedAt: '2026-08-20T10:05:00Z' }]);
    await fixture.whenStable();
    fixture.detectChanges();

    const dialog = fixture.nativeElement.querySelectorAll('dialog')[1] as HTMLDialogElement;
    dialog.showModal = () => dialog.setAttribute('open', '');
    dialog.close = () => dialog.removeAttribute('open');
    const row = fixture.nativeElement.querySelector('tbody tr') as HTMLTableRowElement;
    row.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));
    fixture.detectChanges();
    http.expectOne('http://billing/api/v1/invoices/invoice-1').flush({ id: 'invoice-1', number: 42, status: 'closed', items: [], createdAt: '2026-08-20T10:00:00Z', closedAt: '2026-08-20T10:05:00Z' });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(dialog.open).toBe(true);
    expect(row.tabIndex).toBe(0);
    http.verify();
  });

  it('should update the list immediately when an invoice is closed in the dialog', async () => {
    TestBed.configureTestingModule({ imports: [InvoiceListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: '', billing: 'http://billing/api/v1' } }] });
    const fixture = TestBed.createComponent(InvoiceListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('http://billing/api/v1/invoices').flush([{ id: 'invoice-1', number: 42, status: 'open', itemCount: 1, createdAt: '2026-08-20T10:00:00Z', closedAt: null }]);
    await fixture.whenStable();
    fixture.detectChanges();

    const closedInvoice: Invoice = { id: 'invoice-1', number: 42, status: 'closed', items: [{ productId: 'product-1', productCode: 'PROD-001', productDescription: 'Produto', quantity: 2 }], createdAt: '2026-08-20T10:00:00Z', closedAt: '2026-08-20T10:05:00Z' };
    (fixture.componentInstance as unknown as { onInvoiceClosed(invoice: Invoice): void }).onInvoiceClosed(closedInvoice);
    fixture.detectChanges();

    const row = fixture.nativeElement.querySelector('tbody tr') as HTMLTableRowElement;
    expect(row.textContent).toContain('Fechada');
    expect(row.textContent).not.toContain('Aberta');
    http.verify();
  });
});
