import { OrderItem } from './order-item';

export interface Order {
  orderId: number;
  orderDate: string;
  status: string;
  shippingAddress: string;
  total: number;
  items: OrderItem[];
}
