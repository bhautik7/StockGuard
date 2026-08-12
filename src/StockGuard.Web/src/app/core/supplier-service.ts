import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Supplier } from './supplier';

export interface SupplierPayload {
  name: string;
  contactEmail: string | null;
  contactPhone: string | null;
}

@Injectable({ providedIn: 'root' })
export class SupplierService {
  private apiUrl = 'http://localhost:5270/api/suppliers';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Supplier[]> {
    return this.http.get<Supplier[]>(this.apiUrl);
  }

  create(payload: SupplierPayload): Observable<Supplier> {
    return this.http.post<Supplier>(this.apiUrl, payload);
  }

  update(id: string, payload: SupplierPayload): Observable<Supplier> {
    return this.http.put<Supplier>(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
