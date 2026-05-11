import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { OrdersService } from '../../services/orders.service';
import { AuthService } from '../../services/auth.service';
import { CartItem } from '../../models/cart-item';
import { CheckoutRequest } from '../../models/checkout-request';
import { CheckoutResponse } from '../../models/checkout-response';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './cart.html',
  styleUrl: './cart.css',
})
export class Cart implements OnInit {
  private cartService = inject(CartService);
  private ordersService = inject(OrdersService);
  private authService = inject(AuthService);
  private router = inject(Router);

  cartItems: CartItem[] = [];
  shippingAddress = '';
  city = '';
  postalCode = '';
  phoneNumber = '';
  checkoutResponse: CheckoutResponse | null = null;
  errorMessage = '';

  ngOnInit(): void {
    this.cartService.cartItems$.subscribe((items) => {
      this.cartItems = items;
    });
  }

  getTotal(): number {
    return this.cartItems.reduce((total, item) => {
      return total + item.product.price * item.quantity;
    }, 0);
  }

  getSubtotal(): number {
    return this.getTotal();
  }

  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  increaseQty(productId: number): void {
    this.cartService.increaseQuantity(productId);
  }

  decreaseQty(productId: number): void {
    this.cartService.decreaseQuantity(productId);
  }

  removeItem(productId: number): void {
    this.cartService.removeFromCart(productId);
  }

  placeOrder(): void {
    this.errorMessage = '';
    this.checkoutResponse = null;

    if (!this.authService.isAuthenticated()) {
      this.errorMessage = 'You must be logged in to place an order.';
      this.router.navigate(['/login'], { queryParams: { returnUrl: '/cart' } });
      return;
    }

    if (this.cartItems.length === 0) {
      this.errorMessage = 'Your cart is empty.';
      return;
    }

    if (!this.shippingAddress.trim()) {
      this.errorMessage = 'Shipping address is required.';
      return;
    }

    if (!this.city.trim()) {
      this.errorMessage = 'City is required.';
      return;
    }

    if (!this.postalCode.trim()) {
      this.errorMessage = 'Postal code is required.';
      return;
    }

    if (!this.phoneNumber.trim()) {
      this.errorMessage = 'Phone number is required.';
      return;
    }

    const request: CheckoutRequest = {
      shippingAddress: this.shippingAddress,
      city: this.city,
      postalCode: this.postalCode,
      phoneNumber: this.phoneNumber,
      items: this.cartItems.map((item) => ({
        productId: item.product.id,
        quantity: item.quantity,
      })),
    };

    this.ordersService.placeOrder(request).subscribe({
      next: (response) => {
        this.checkoutResponse = response;
        this.shippingAddress = '';
        this.city = '';
        this.postalCode = '';
        this.phoneNumber = '';
        this.cartService.clearCart();
      },
      error: (error) => {
        console.error('Order creation failed:', error);
        console.error('Backend validation details:', error?.error);

        if (error?.status === 401) {
          this.errorMessage = 'Your session expired. Please login again.';
          return;
        }

        this.errorMessage = this.getApiErrorMessage(error);
      },
    });
  }

  private getApiErrorMessage(error: any): string {
    const apiError = error?.error;

    if (typeof apiError === 'string' && apiError.trim()) {
      return apiError;
    }

    if (apiError?.message && typeof apiError.message === 'string') {
      return apiError.message;
    }

    const validationErrors = apiError?.errors;
    if (validationErrors && typeof validationErrors === 'object') {
      const firstError = Object.values(validationErrors)
        .flat()
        .find((msg) => typeof msg === 'string' && msg.trim().length > 0) as string | undefined;

      if (firstError) {
        return firstError;
      }
    }

    return 'Failed to place order.';
  }
}
