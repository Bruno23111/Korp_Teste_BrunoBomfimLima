import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  username: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private static readonly tokenKey = 'korp-access-token';
  private static readonly userKey = 'korp-auth-user';
  private static readonly expirationKey = 'korp-access-token-expiration';

  login(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('http://localhost:5290/api/auth/login', { username, password }).pipe(
      tap((response) => {
        localStorage.setItem(AuthService.tokenKey, response.accessToken);
        localStorage.setItem(AuthService.userKey, JSON.stringify({ username: response.username, role: response.role }));
        localStorage.setItem(AuthService.expirationKey, response.expiresAt);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(AuthService.tokenKey);
    localStorage.removeItem(AuthService.userKey);
    localStorage.removeItem(AuthService.expirationKey);
  }

  get token(): string | null {
    const token = localStorage.getItem(AuthService.tokenKey);
    if (!token) {
      return null;
    }

    const expiration = localStorage.getItem(AuthService.expirationKey);
    if (expiration && Date.parse(expiration) <= Date.now()) {
      this.logout();
      return null;
    }

    return token;
  }

  get user(): { username: string; role: string } | null {
    const value = localStorage.getItem(AuthService.userKey);
    return value ? JSON.parse(value) : null;
  }
}
