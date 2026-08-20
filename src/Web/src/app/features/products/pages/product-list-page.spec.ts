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
    expect(fixture.nativeElement.textContent).toContain('Página 1 de 1');
    expect(fixture.nativeElement.querySelector('[aria-label="Paginação de produtos"]')).toBeNull();
    http.verify();
  });

  it('should display pagination controls when there is more than one page', async () => {
    TestBed.configureTestingModule({ imports: [ProductListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: 'http://inventory/api/v1', billing: '' } }] });
    const fixture = TestBed.createComponent(ProductListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((request) => request.url === 'http://inventory/api/v1/products').flush({ items: [{ id: 'product-1', code: 'PROD-001', description: 'Produto', balance: 8 }], totalRecords: 21, pageNumber: 1, pageSize: 20, totalPages: 2 });
    await fixture.whenStable();
    fixture.detectChanges();

    const pagination = fixture.nativeElement.querySelector('[aria-label="Paginação de produtos"]') as HTMLElement;
    expect(pagination).not.toBeNull();
    expect(pagination.textContent).toContain('Página 1 de 2');
    http.verify();
  });

  it('should open the product form in a dialog', () => {
    TestBed.configureTestingModule({ imports: [ProductListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: 'http://inventory/api/v1', billing: '' } }] });
    const fixture = TestBed.createComponent(ProductListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((request) => request.url === 'http://inventory/api/v1/products').flush({ items: [], totalRecords: 0, pageNumber: 1, pageSize: 20, totalPages: 0 });
    fixture.detectChanges();

    const createButton = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    expect(createButton.textContent).toContain('Novo produto');
    expect(createButton.querySelector('svg')?.getAttribute('aria-hidden')).toBe('true');
    const dialog = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    dialog.showModal = () => dialog.setAttribute('open', '');
    dialog.close = () => dialog.removeAttribute('open');
    createButton.click();
    fixture.detectChanges();

    expect(dialog.open).toBe(true);
    expect(dialog.textContent).toContain('Novo produto');
    expect(dialog.querySelector('form')).not.toBeNull();
    http.verify();
  });

  it('should highlight products without stock', async () => {
    TestBed.configureTestingModule({ imports: [ProductListPage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: 'http://inventory/api/v1', billing: '' } }] });
    const fixture = TestBed.createComponent(ProductListPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((request) => request.url === 'http://inventory/api/v1/products').flush({ items: [{ id: 'product-empty', code: 'PROD-000', description: 'Produto zerado', balance: 0 }], totalRecords: 1, pageNumber: 1, pageSize: 20, totalPages: 1 });
    await fixture.whenStable();
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('[aria-label="Saldo zero, sem estoque"]') as HTMLElement;
    expect(badge.textContent).toContain('Sem estoque');
    http.verify();
  });
});
