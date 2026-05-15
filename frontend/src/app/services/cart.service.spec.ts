import { TestBed } from '@angular/core/testing';
import { CartService } from './cart.service';

describe('CartService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('starts with an empty cart', () => {
    const service = TestBed.inject(CartService);
    expect(service.getCartItems().length).toBe(0);
  });

  it('adds a new line item with quantity 1', () => {
    const service = TestBed.inject(CartService);
    service.addToCart({ id: 1, name: 'Widget', price: 49.99 });
    const items = service.getCartItems();
    expect(items.length).toBe(1);
    expect(items[0].quantity).toBe(1);
    expect(items[0].product.name).toBe('Widget');
  });

  it('increments quantity when adding the same product again', () => {
    const service = TestBed.inject(CartService);
    const product = { id: 2, name: 'Gadget', price: 10 };
    service.addToCart(product);
    service.addToCart(product);
    expect(service.getCartItems()[0].quantity).toBe(2);
  });

  it('clears the cart', () => {
    const service = TestBed.inject(CartService);
    service.addToCart({ id: 3, name: 'Item', price: 5 });
    service.clearCart();
    expect(service.getCartItems().length).toBe(0);
  });
});
