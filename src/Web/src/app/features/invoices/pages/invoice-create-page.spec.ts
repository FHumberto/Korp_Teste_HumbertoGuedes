import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { InvoiceCreatePage } from './invoice-create-page';

describe('InvoiceCreatePage', () => {
  it('should load products from the dedicated available-products route', async () => {
    TestBed.configureTestingModule({ imports: [InvoiceCreatePage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: 'http://inventory/api/v1', billing: 'http://billing/api/v1' } }] });
    const fixture = TestBed.createComponent(InvoiceCreatePage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('http://inventory/api/v1/products/available').flush([{ id: 'product-available', code: 'COM-SALDO', description: 'Com saldo', balance: 3 }]);
    await fixture.whenStable();
    fixture.detectChanges();

    const options = Array.from((fixture.nativeElement.querySelector('#product-0') as HTMLSelectElement).options);
    expect(options.some((option) => option.value === 'product-available')).toBe(true);
    http.verify();
  });

  it('should add another item and prevent selecting an already used product', async () => {
    TestBed.configureTestingModule({ imports: [InvoiceCreatePage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: 'http://inventory/api/v1', billing: 'http://billing/api/v1' } }] });
    const fixture = TestBed.createComponent(InvoiceCreatePage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('http://inventory/api/v1/products/available').flush([{ id: 'product-1', code: 'PROD-001', description: 'Produto', balance: 10 }, { id: 'product-2', code: 'PROD-002', description: 'Outro', balance: 5 }]);
    await fixture.whenStable();
    fixture.detectChanges();

    const firstSelect = fixture.nativeElement.querySelector('#product-0') as HTMLSelectElement;
    firstSelect.value = 'product-1';
    firstSelect.dispatchEvent(new Event('input', { bubbles: true }));
    firstSelect.dispatchEvent(new Event('change', { bubbles: true }));
    await fixture.whenStable();
    fixture.detectChanges();
    const addButton = Array.from(fixture.nativeElement.querySelectorAll('button')).find((button) => (button as HTMLButtonElement).textContent?.includes('Adicionar produto')) as HTMLButtonElement;
    addButton.click();
    fixture.detectChanges();

    const secondSelect = fixture.nativeElement.querySelector('#product-1') as HTMLSelectElement;
    const duplicateOption = Array.from(secondSelect.options).find((option) => option.value === 'product-1');
    expect(duplicateOption?.disabled).toBe(true);
    http.verify();
  });
});
