export interface Product {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  unit: string;
  reorderLevel: number;
  categoryId: string;
  categoryName: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}