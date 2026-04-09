import { Component } from '@angular/core';
import { ProductList } from './components/product-list/product-list';
import { Cart } from './components/cart/cart';

@Component({
  selector: 'app-root',
  imports: [ProductList, Cart],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  title = 'frontend';
}
