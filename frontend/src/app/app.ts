import { Component } from '@angular/core';
import { ProductList } from './components/product-list/product-list';
import { Cart } from './components/cart/cart';
import { CheckoutComponent } from './components/checkout/checkout';
import { RegisterComponent } from './components/register/register';
import { LoginComponent } from './components/login/login';

@Component({
  selector: 'app-root',
  imports: [ProductList, Cart, CheckoutComponent, RegisterComponent, LoginComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  title = 'frontend';
}
