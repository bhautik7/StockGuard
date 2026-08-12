export interface ReservationLine {
  inventoryBatchId: string;
  quantity: number;
}

export interface Reservation {
  id: string;
  status: string;
  lines: ReservationLine[];
}