import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CheckoutRequest } from '../models/checkout-request';
import { CheckoutResponse } from '../models/checkout-response';

@Injectable({
  providedIn: 'root',
})
export class CheckoutService {
  private apiUrl = 'http://localhost:5261/api/checkout';

  constructor(private http: HttpClient) {}

  placeOrder(request: CheckoutRequest): Observable<CheckoutResponse> {
    return this.http.post<CheckoutResponse>(this.apiUrl, request);
  }
}
