import { CheckoutItemResponse } from './checkout-item-response';

export interface CheckoutResponse {
  shippingAddress: string;
  items: CheckoutItemResponse[];
  total: number;
  message: string;
}
