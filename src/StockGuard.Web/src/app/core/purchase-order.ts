export interface PurchaseOrderLine {
  productId: string;
  productName: string;
  quantityOrdered: number;
  quantityReceived: number;
  expectedDeliveryDate: string;
}

export interface PurchaseOrder {
  id: string;
  orderNumber: string;
  supplierId: string;
  supplierName: string;
  status: 'Draft' | 'Submitted' | 'Approved' | 'PartiallyReceived' | 'Received' | 'Cancelled' | string;
  lines: PurchaseOrderLine[];
}
