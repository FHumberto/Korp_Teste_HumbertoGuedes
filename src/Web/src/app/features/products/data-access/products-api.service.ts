import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { CreatedProduct, CreateProductRequest, Paged, Product, ProductSummary } from '../models/product.models';

@Service()
export class ProductsApiService {
  private readonly http = inject(HttpClient);
  private readonly endpoints = inject(API_ENDPOINTS);

  create(request: CreateProductRequest): Observable<CreatedProduct> {
    return this.http.post<CreatedProduct>(`${this.endpoints.inventory}/products`, request);
  }

  list(pageNumber = 1, pageSize = 20): Observable<Paged<ProductSummary>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<Paged<ProductSummary>>(`${this.endpoints.inventory}/products`, { params });
  }

  getById(productId: string): Observable<Product> {
    return this.http.get<Product>(`${this.endpoints.inventory}/products/${productId}`);
  }
}
