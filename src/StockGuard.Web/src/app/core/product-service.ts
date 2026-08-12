import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult, Product } from './product';

export interface ProductPayload {
  sku: string;
  name: string;
  description: string | null;
  unit: string;
  reorderLevel: number;
  categoryId: string;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = 'http://localhost:5270/api/products';

  constructor(private http: HttpClient) {}

  getPaged(page: number, pageSize: number, search: string, categoryId?: string): Observable<PagedResult<Product>> {
    const params = new URLSearchParams({ page: page.toString(), pageSize: pageSize.toString() });
    if (search) params.set('search', search);
    if (categoryId) params.set('categoryId', categoryId);
    return this.http.get<PagedResult<Product>>(`${this.apiUrl}?${params.toString()}`);
  }

  create(payload: ProductPayload): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, payload);
  }

  update(id: string, payload: ProductPayload): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
