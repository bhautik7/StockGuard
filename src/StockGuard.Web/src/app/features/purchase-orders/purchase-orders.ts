import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PurchaseOrderService, CreatePurchaseOrderLinePayload } from '../../core/purchase-order-service';
import { PurchaseOrder } from '../../core/purchase-order';
import { SupplierService } from '../../core/supplier-service';
import { Supplier } from '../../core/supplier';
import { ProductService } from '../../core/product-service';
import { Product } from '../../core/product';
import { WarehouseService } from '../../core/warehouse-service';
import { Warehouse } from '../../core/warehouse';
import { Auth } from '../../core/auth';
import { Modal } from '../../shared/modal/modal';

interface DraftLine extends CreatePurchaseOrderLinePayload {
  productName: string;
}

interface ReceiveLineForm {
  productId: string;
  productName: string;
  remaining: number;
  quantityReceived: number;
  batchNumber: string;
  expiryDate: string;
  warehouseId: string;
}

@Component({
  selector: 'app-purchase-orders',
  standalone: true,
  imports: [FormsModule, Modal],
  templateUrl: './purchase-orders.html',
  styleUrl: './purchase-orders.scss'
})
export class PurchaseOrders implements OnInit {
  orders = signal<PurchaseOrder[]>([]);
  loading = signal(false);
  expandedId = signal<string | null>(null);

  suppliers = signal<Supplier[]>([]);
  warehouses = signal<Warehouse[]>([]);

  showCreate = signal(false);
  createSupplierId = '';
  draftLines = signal<DraftLine[]>([]);
  lineProductQuery = '';
  lineProductSuggestions = signal<Product[]>([]);
  lineQuantity = 1;
  lineExpectedDate = '';
  createError = signal<string | null>(null);
  creating = signal(false);
  private searchTimer?: ReturnType<typeof setTimeout>;

  actionError = signal<string | null>(null);
  actionBusyId = signal<string | null>(null);

  showReceive = signal(false);
  receiveOrder: PurchaseOrder | null = null;
  receiveLines: ReceiveLineForm[] = [];
  receiveError = signal<string | null>(null);
  receiving = signal(false);

  constructor(
    private poService: PurchaseOrderService,
    private supplierService: SupplierService,
    private productService: ProductService,
    private warehouseService: WarehouseService,
    public auth: Auth
  ) {}

  ngOnInit(): void {
    this.load();
    this.supplierService.getAll().subscribe(suppliers => this.suppliers.set(suppliers));
    this.warehouseService.getAll().subscribe(warehouses => this.warehouses.set(warehouses));
  }

  canCreateOrCancel(): boolean {
    return this.auth.hasRole('Administrator', 'PurchasingOfficer');
  }

  canApprove(): boolean {
    return this.auth.hasRole('Administrator', 'InventoryManager');
  }

  canReceive(): boolean {
    return this.auth.hasRole('Administrator', 'InventoryManager', 'WarehouseEmployee');
  }

  load(): void {
    this.loading.set(true);
    this.poService.getAll().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  toggleExpand(order: PurchaseOrder): void {
    this.expandedId.set(this.expandedId() === order.id ? null : order.id);
  }

  statusBadge(status: string): string {
    switch (status) {
      case 'Draft': return 'badge-neutral';
      case 'Submitted': return 'badge-info';
      case 'Approved': return 'badge-success';
      case 'PartiallyReceived': return 'badge-warning';
      case 'Received': return 'badge-success';
      case 'Cancelled': return 'badge-danger';
      default: return 'badge-neutral';
    }
  }

  // --- Create ---

  openCreate(): void {
    this.createSupplierId = this.suppliers()[0]?.id ?? '';
    this.draftLines.set([]);
    this.lineProductQuery = '';
    this.lineProductSuggestions.set([]);
    this.lineQuantity = 1;
    this.lineExpectedDate = '';
    this.createError.set(null);
    this.showCreate.set(true);
  }

  closeCreate(): void {
    this.showCreate.set(false);
  }

  onLineProductQueryChange(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
    if (!this.lineProductQuery.trim()) {
      this.lineProductSuggestions.set([]);
      return;
    }
    this.searchTimer = setTimeout(() => {
      this.productService.getPaged(1, 6, this.lineProductQuery).subscribe(result => this.lineProductSuggestions.set(result.items));
    }, 250);
  }

  addLine(product: Product): void {
    if (this.draftLines().some(l => l.productId === product.id)) return;
    this.draftLines.update(lines => [
      ...lines,
      { productId: product.id, productName: product.name, quantityOrdered: this.lineQuantity || 1, expectedDeliveryDate: this.lineExpectedDate || this.defaultDeliveryDate() }
    ]);
    this.lineProductQuery = '';
    this.lineProductSuggestions.set([]);
    this.lineQuantity = 1;
  }

  removeLine(productId: string): void {
    this.draftLines.update(lines => lines.filter(l => l.productId !== productId));
  }

  private defaultDeliveryDate(): string {
    const d = new Date();
    d.setDate(d.getDate() + 14);
    return d.toISOString().slice(0, 10);
  }

  submitCreate(): void {
    if (this.draftLines().length === 0) {
      this.createError.set('Add at least one line to the order.');
      return;
    }
    this.createError.set(null);
    this.creating.set(true);

    this.poService.create({
      supplierId: this.createSupplierId,
      lines: this.draftLines().map(({ productId, quantityOrdered, expectedDeliveryDate }) => ({ productId, quantityOrdered, expectedDeliveryDate }))
    }).subscribe({
      next: () => {
        this.creating.set(false);
        this.showCreate.set(false);
        this.load();
      },
      error: (err) => {
        this.creating.set(false);
        this.createError.set(this.extractError(err));
      }
    });
  }

  // --- Lifecycle actions ---

  submit(order: PurchaseOrder): void {
    this.actionError.set(null);
    this.actionBusyId.set(order.id);
    this.poService.submit(order.id).subscribe({
      next: () => { this.actionBusyId.set(null); this.load(); },
      error: (err) => { this.actionBusyId.set(null); this.actionError.set(this.extractError(err)); }
    });
  }

  approve(order: PurchaseOrder): void {
    this.actionError.set(null);
    this.actionBusyId.set(order.id);
    this.poService.approve(order.id).subscribe({
      next: () => { this.actionBusyId.set(null); this.load(); },
      error: (err) => { this.actionBusyId.set(null); this.actionError.set(this.extractError(err)); }
    });
  }

  cancel(order: PurchaseOrder): void {
    if (!confirm(`Cancel purchase order ${order.orderNumber}?`)) return;
    this.actionError.set(null);
    this.actionBusyId.set(order.id);
    this.poService.cancel(order.id).subscribe({
      next: () => { this.actionBusyId.set(null); this.load(); },
      error: (err) => { this.actionBusyId.set(null); this.actionError.set(this.extractError(err)); }
    });
  }

  // --- Receive ---

  openReceive(order: PurchaseOrder): void {
    this.receiveOrder = order;
    this.receiveLines = order.lines
      .filter(l => l.quantityReceived < l.quantityOrdered)
      .map(l => ({
        productId: l.productId,
        productName: l.productName,
        remaining: l.quantityOrdered - l.quantityReceived,
        quantityReceived: l.quantityOrdered - l.quantityReceived,
        batchNumber: `${order.orderNumber}-${l.productId.slice(0, 4)}`,
        expiryDate: '',
        warehouseId: this.warehouses()[0]?.id ?? ''
      }));
    this.receiveError.set(null);
    this.showReceive.set(true);
  }

  closeReceive(): void {
    this.showReceive.set(false);
    this.receiveOrder = null;
  }

  submitReceive(): void {
    if (!this.receiveOrder) return;
    this.receiveError.set(null);
    this.receiving.set(true);

    const lines = this.receiveLines
      .filter(l => l.quantityReceived > 0)
      .map(l => ({
        productId: l.productId,
        quantityReceived: Number(l.quantityReceived),
        batchNumber: l.batchNumber,
        expiryDate: l.expiryDate,
        warehouseId: l.warehouseId
      }));

    this.poService.receive(this.receiveOrder.id, { lines }).subscribe({
      next: () => {
        this.receiving.set(false);
        this.showReceive.set(false);
        this.load();
      },
      error: (err) => {
        this.receiving.set(false);
        this.receiveError.set(this.extractError(err));
      }
    });
  }

  private extractError(err: any): string {
    return err?.error && typeof err.error === 'string' ? err.error : 'Something went wrong. Please try again.';
  }
}
