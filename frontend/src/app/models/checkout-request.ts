import { CheckoutItemRequest } from './checkout-item-request';

export interface CheckoutRequest {
  shippingAddress: string;
  items: CheckoutItemRequest[];
}
