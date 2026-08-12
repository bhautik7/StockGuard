export interface InventoryBatch {
  id: string;
  productId: string;
  productName: string;
  warehouseId: string;
  warehouseName: string;
  batchNumber: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
  expiryDate: string;
  status: 'Available' | 'Quarantined' | 'Expired' | string;
}
