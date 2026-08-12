import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InventoryBatch } from './inventory-batch';

export interface ReceiveInventoryPayload {
  productId: string;
  warehouseId: string;
  batchNumber: string;
  quantity: number;
  expiryDate: string;
}

export interface TransferInventoryPayload {
  inventoryBatchId: string;
  toWarehouseId: string;
  quantity: number;
}

export interface AdjustInventoryPayload {
  inventoryBatchId: string;
  quantityChange: number;
  reason: string;
}

export interface QuarantineInventoryPayload {
  inventoryBatchId: string;
  reason: string;
}

@Injectable({ providedIn: 'root' })
export class InventoryBatchService {
  private apiUrl = 'http://localhost:5270/api/inventorybatches';

  constructor(private http: HttpClient) {}

  getByProduct(productId: string): Observable<InventoryBatch[]> {
    return this.http.get<InventoryBatch[]>(`${this.apiUrl}/by-product/${productId}`);
  }

  receive(payload: ReceiveInventoryPayload): Observable<InventoryBatch> {
    return this.http.post<InventoryBatch>(`${this.apiUrl}/receive`, payload);
  }

  transfer(payload: TransferInventoryPayload): Observable<InventoryBatch> {
    return this.http.post<InventoryBatch>(`${this.apiUrl}/transfer`, payload);
  }

  adjust(payload: AdjustInventoryPayload): Observable<InventoryBatch> {
    return this.http.post<InventoryBatch>(`${this.apiUrl}/adjust`, payload);
  }

  quarantine(payload: QuarantineInventoryPayload): Observable<InventoryBatch> {
    return this.http.post<InventoryBatch>(`${this.apiUrl}/quarantine`, payload);
  }
}
