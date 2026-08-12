import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Warehouse } from './warehouse';

export interface WarehousePayload {
  name: string;
  location: string;
}

@Injectable({ providedIn: 'root' })
export class WarehouseService {
  private apiUrl = 'http://localhost:5270/api/warehouses';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(this.apiUrl);
  }

  create(payload: WarehousePayload): Observable<Warehouse> {
    return this.http.post<Warehouse>(this.apiUrl, payload);
  }

  update(id: string, payload: WarehousePayload): Observable<Warehouse> {
    return this.http.put<Warehouse>(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
