import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
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

    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    dialog.showModal = () => dialog.setAttribute('open', '');
    dialog.close = () => dialog.removeAttribute('open');
    fixture.nativeElement.querySelector('button').click();
    fixture.detectChanges();

    expect(dialog.open).toBe(true);
    expect(dialog.textContent).toContain('Nova nota fiscal');
    http.expectOne((request) => request.url === 'http://inventory/api/v1/products').flush({ items: [{ id: 'product-1', code: 'PROD-001', description: 'Produto', balance: 10 }], totalRecords: 1, pageNumber: 1, pageSize: 100, totalPages: 1 });
    fixture.detectChanges();
    expect(dialog.querySelector('form')).not.toBeNull();
    http.verify();
  });
});
