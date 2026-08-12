import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult, Product } from './product';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = 'http://localhost:5270/api/products';

  constructor(private http: HttpClient) {}

  getPaged(page: number, pageSize: number, search: string): Observable<PagedResult<Product>> {
    const params = new URLSearchParams({ page: page.toString(), pageSize: pageSize.toString() });
    if (search) params.set('search', search);
    return this.http.get<PagedResult<Product>>(`${this.apiUrl}?${params.toString()}`);
  }
}