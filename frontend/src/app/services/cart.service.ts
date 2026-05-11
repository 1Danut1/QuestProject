import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Product } from '../models/product';
import { CartItem } from '../models/cart-item';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private cartItemsSubject = new BehaviorSubject<CartItem[]>([]);
  cartItems$ = this.cartItemsSubject.asObservable();

  addToCart(product: Product): void {
    const currentItems = this.cartItemsSubject.value;

    const existingItem = currentItems.find((item) => item.product.id === product.id);

    if (existingItem) {
      existingItem.quantity += 1;
      this.cartItemsSubject.next([...currentItems]);
    } else {
      const newItem: CartItem = {
        product,
        quantity: 1,
      };

      this.cartItemsSubject.next([...currentItems, newItem]);
    }
  }

  getCartItems(): CartItem[] {
    return this.cartItemsSubject.value;
  }

  increaseQuantity(productId: number): void {
    const updatedItems = this.cartItemsSubject.value.map((item) =>
      item.product.id === productId ? { ...item, quantity: item.quantity + 1 } : item,
    );

    this.cartItemsSubject.next(updatedItems);
  }

  decreaseQuantity(productId: number): void {
    const updatedItems = this.cartItemsSubject.value
      .map((item) =>
        item.product.id === productId ? { ...item, quantity: item.quantity - 1 } : item,
      )
      .filter((item) => item.quantity > 0);

    this.cartItemsSubject.next(updatedItems);
  }

  removeFromCart(productId: number): void {
    const updatedItems = this.cartItemsSubject.value.filter((item) => item.product.id !== productId);
    this.cartItemsSubject.next(updatedItems);
  }

  clearCart(): void {
    this.cartItemsSubject.next([]);
  }
}
