import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { ProductListPage } from './product-list-page';

describe('ProductListPage', () => {
  it('should render products returned by the API', async () => {
    TestBed.configureTestingModule({ imports: [ProductListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: 'http://inventory/api/v1', billing: '' } }] });
    const fixture = TestBed.createComponent(ProductListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((request) => request.url === 'http://inventory/api/v1/products').flush({ items: [{ id: crypto.randomUUID(), code: 'PROD-001', description: 'Produto de teste', balance: 8 }], totalRecords: 1, pageNumber: 1, pageSize: 20, totalPages: 1 });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('PROD-001');
    expect(fixture.nativeElement.textContent).toContain('Produto de teste');
    http.verify();
  });
});
