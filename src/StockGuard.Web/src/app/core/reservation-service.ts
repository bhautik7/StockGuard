import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Reservation } from './reservation';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private apiUrl = 'http://localhost:5270/api/reservations';

  constructor(private http: HttpClient) {}

  reserve(productId: string, quantity: number): Observable<Reservation> {
    const idempotencyKey = crypto.randomUUID();
    return this.http.post<Reservation>(this.apiUrl, { productId, quantity, idempotencyKey });
  }
}