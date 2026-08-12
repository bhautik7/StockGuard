import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RegisterUserPayload {
  email: string;
  password: string;
  fullName: string;
  role: string;
}

export interface RegisteredUser {
  userId: string;
  email: string;
  fullName: string;
  role: string;
  token: string;
  refreshToken: string;
}

export const ALL_ROLES = ['Administrator', 'InventoryManager', 'PurchasingOfficer', 'WarehouseEmployee', 'Auditor'];

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = 'http://localhost:5270/api/auth';

  constructor(private http: HttpClient) {}

  register(payload: RegisterUserPayload): Observable<RegisteredUser> {
    return this.http.post<RegisteredUser>(`${this.apiUrl}/register`, payload);
  }
}
