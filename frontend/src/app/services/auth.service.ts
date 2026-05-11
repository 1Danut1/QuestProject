import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';

import { RegisterRequest } from '../models/register-request';
import { LoginRequest } from '../models/login-request';
import { AuthResponse } from '../models/auth-response';
import { AuthUser } from '../models/auth-user';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'http://localhost:5261/api/auth';
  private readonly storageKey = 'quest_auth_user';
  private authUserSubject = new BehaviorSubject<AuthUser | null>(this.getStoredUser());
  authUser$ = this.authUserSubject.asObservable();
  isAuthenticated$ = this.authUser$.pipe(map((user) => !!user?.token));

  constructor(private http: HttpClient) {}

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap((response) => {
        const user: AuthUser = { username: response.username, token: response.token };
        this.authUserSubject.next(user);
        localStorage.setItem(this.storageKey, JSON.stringify(user));
      }),
    );
  }

  logout(): void {
    this.authUserSubject.next(null);
    localStorage.removeItem(this.storageKey);
  }

  getCurrentUser(): AuthUser | null {
    return this.authUserSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.authUserSubject.value?.token;
  }

  getToken(): string | null {
    return this.authUserSubject.value?.token ?? null;
  }

  private getStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(this.storageKey);
    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as AuthUser;
      if (!parsed?.username || !parsed?.token) {
        localStorage.removeItem(this.storageKey);
        return null;
      }

      return parsed;
    } catch {
      localStorage.removeItem(this.storageKey);
      return null;
    }
  }
}
