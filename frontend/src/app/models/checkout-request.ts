import { CheckoutItemRequest } from './checkout-item-request';

export interface CheckoutRequest {
  shippingAddress: string;
  city: string;
  postalCode: string;
  phoneNumber: string;
  items: CheckoutItemRequest[];
}
