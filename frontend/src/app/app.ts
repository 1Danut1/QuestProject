import { Component } from '@angular/core';
import { ProductList } from './components/product-list/product-list';
import { Cart } from './components/cart/cart';
import { CheckoutComponent } from './components/checkout/checkout';

@Component({
  selector: 'app-root',
  imports: [ProductList, Cart, CheckoutComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  title = 'frontend';
}
