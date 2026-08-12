import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

interface AuthResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
  token: string;
  refreshToken: string;
}

@Injectable({ providedIn: 'root' })
export class Auth {
  private apiUrl = 'http://localhost:5270/api/auth';
  currentUser = signal<AuthResponse | null>(null);

  constructor(private http: HttpClient) {
    const stored = localStorage.getItem('stockguard_user');
    if (stored) this.currentUser.set(JSON.parse(stored));
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap(response => {
        this.currentUser.set(response);
        localStorage.setItem('stockguard_user', JSON.stringify(response));
      })
    );
  }

  logout(): void {
    this.currentUser.set(null);
    localStorage.removeItem('stockguard_user');
  }

  getToken(): string | null {
    return this.currentUser()?.token ?? null;
  }

  isLoggedIn(): boolean {
    return this.currentUser() !== null;
  }
}