import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PurchaseOrder } from './purchase-order';

export interface CreatePurchaseOrderLinePayload {
  productId: string;
  quantityOrdered: number;
  expectedDeliveryDate: string;
}

export interface CreatePurchaseOrderPayload {
  supplierId: string;
  lines: CreatePurchaseOrderLinePayload[];
}

export interface ReceivePurchaseOrderLinePayload {
  productId: string;
  quantityReceived: number;
  batchNumber: string;
  expiryDate: string;
  warehouseId: string;
}

export interface ReceivePurchaseOrderPayload {
  lines: ReceivePurchaseOrderLinePayload[];
}

@Injectable({ providedIn: 'root' })
export class PurchaseOrderService {
  private apiUrl = 'http://localhost:5270/api/purchaseorders';

  constructor(private http: HttpClient) {}

  getAll(): Observable<PurchaseOrder[]> {
    return this.http.get<PurchaseOrder[]>(this.apiUrl);
  }

  getById(id: string): Observable<PurchaseOrder> {
    return this.http.get<PurchaseOrder>(`${this.apiUrl}/${id}`);
  }

  create(payload: CreatePurchaseOrderPayload): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(this.apiUrl, payload);
  }

  submit(id: string): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(`${this.apiUrl}/${id}/submit`, {});
  }

  approve(id: string): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(`${this.apiUrl}/${id}/approve`, {});
  }

  cancel(id: string): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(`${this.apiUrl}/${id}/cancel`, {});
  }

  receive(id: string, payload: ReceivePurchaseOrderPayload): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(`${this.apiUrl}/${id}/receive`, payload);
  }
}
