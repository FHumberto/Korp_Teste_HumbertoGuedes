import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { InvoicesApiService } from './invoices-api.service';

describe('InvoicesApiService', () => {
  let api: InvoicesApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_ENDPOINTS, useValue: { inventory: '', billing: 'http://billing/api/v1' } }] });
    api = TestBed.inject(InvoicesApiService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('list should send the selected status', () => {
    api.list('open').subscribe();
    const request = http.expectOne((candidate) => candidate.url === 'http://billing/api/v1/invoices');
    expect(request.request.params.get('status')).toBe('open');
    request.flush([]);
  });

  it('create should post only product ids and quantities', () => {
    const body = { items: [{ productId: 'product-1', quantity: 2 }] };
    api.create(body).subscribe();
    const request = http.expectOne('http://billing/api/v1/invoices');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ id: 'invoice-1', number: 1, status: 'open', items: [], createdAt: new Date().toISOString(), closedAt: null });
  });

  it('getDocument should request the invoice PDF as a blob', () => {
    api.getDocument('invoice-1').subscribe();
    const request = http.expectOne('http://billing/api/v1/invoices/invoice-1/document.pdf');
    expect(request.request.method).toBe('GET');
    expect(request.request.responseType).toBe('blob');
    request.flush(new Blob(['pdf'], { type: 'application/pdf' }));
  });
});
