import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { ProductsApiService } from './products-api.service';

describe('ProductsApiService', () => {
  let api: ProductsApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_ENDPOINTS, useValue: { inventory: 'http://inventory/api/v1', billing: 'http://billing/api/v1' } }] });
    api = TestBed.inject(ProductsApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('list should send pagination parameters', () => {
    api.list(2, 20).subscribe();
    const request = http.expectOne((candidate) => candidate.url === 'http://inventory/api/v1/products');
    expect(request.request.params.get('pageNumber')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('20');
    request.flush({ items: [], totalRecords: 0, pageNumber: 2, pageSize: 20, totalPages: 0 });
  });

  it('create should post the product contract', () => {
    const body = { code: 'PROD-001', description: 'Produto', initialBalance: 10 };
    api.create(body).subscribe();
    const request = http.expectOne('http://inventory/api/v1/products');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ id: crypto.randomUUID(), code: body.code, description: body.description, balance: 10, createdAt: new Date().toISOString() });
  });

  it('listAll should load every available page', () => {
    let result: readonly unknown[] = [];
    api.listAll().subscribe((products) => result = products);
    http.expectOne((request) => request.params.get('pageNumber') === '1' && request.params.get('pageSize') === '100').flush({ items: [{ id: '1', code: 'A', description: 'A', balance: 1 }], totalRecords: 101, pageNumber: 1, pageSize: 100, totalPages: 2 });
    http.expectOne((request) => request.params.get('pageNumber') === '2' && request.params.get('pageSize') === '100').flush({ items: [{ id: '2', code: 'B', description: 'B', balance: 2 }], totalRecords: 101, pageNumber: 2, pageSize: 100, totalPages: 2 });
    expect(result).toHaveLength(2);
  });
});
