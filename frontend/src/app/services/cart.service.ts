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
}
