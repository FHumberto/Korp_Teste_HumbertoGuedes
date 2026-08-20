import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { ProductCreatePage } from './product-create-page';

describe('ProductCreatePage', () => {
  it('should reject an empty product form before calling the API', async () => {
    TestBed.configureTestingModule({ imports: [ProductCreatePage], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_ENDPOINTS, useValue: { inventory: 'http://inventory/api/v1', billing: '' } }] });
    const fixture = TestBed.createComponent(ProductCreatePage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();

    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('O código do produto é obrigatório.');
    expect(fixture.nativeElement.textContent).toContain('A descrição do produto é obrigatória.');
    http.expectNone('http://inventory/api/v1/products');
    http.verify();
  });
});
