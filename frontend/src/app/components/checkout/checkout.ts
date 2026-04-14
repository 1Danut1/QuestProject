import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { CartService } from '../../services/cart.service';
import { CheckoutService } from '../../services/checkout.service';

import { CartItem } from '../../models/cart-item';
import { CheckoutRequest } from '../../models/checkout-request';
import { CheckoutResponse } from '../../models/checkout-response';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class CheckoutComponent implements OnInit {
  cartItems: CartItem[] = [];
  shippingAddress: string = '';
  checkoutResponse: CheckoutResponse | null = null;
  errorMessage: string = '';

  constructor(
    private cartService: CartService,
    private checkoutService: CheckoutService,
  ) {}

  ngOnInit(): void {
    this.cartService.cartItems$.subscribe((items) => {
      this.cartItems = items;
    });
  }

  placeOrder(): void {
    this.errorMessage = '';
    this.checkoutResponse = null;

    if (!this.shippingAddress.trim()) {
      this.errorMessage = 'Shipping address is required.';
      return;
    }

    if (this.cartItems.length === 0) {
      this.errorMessage = 'Your cart is empty.';
      return;
    }

    const request: CheckoutRequest = {
      shippingAddress: this.shippingAddress,
      items: this.cartItems.map((item) => ({
        productId: item.product.id,
        quantity: item.quantity,
      })),
    };

    this.checkoutService.placeOrder(request).subscribe({
      next: (response) => {
        this.checkoutResponse = response;
        this.shippingAddress = '';
        this.cartService.clearCart();
      },
      error: (error) => {
        console.error('Checkout error:', error);
        this.errorMessage = 'Failed to place order.';
      },
    });
  }
}
