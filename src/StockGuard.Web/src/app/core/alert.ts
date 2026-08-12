export interface Alert {
  id: string;
  type: string;
  message: string;
  createdAtUtc: string;
  isResolved: boolean;
}