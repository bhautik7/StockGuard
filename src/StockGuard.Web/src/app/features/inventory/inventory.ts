import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../core/product-service';
import { Product } from '../../core/product';
import { WarehouseService } from '../../core/warehouse-service';
import { Warehouse } from '../../core/warehouse';
import { InventoryBatchService } from '../../core/inventory-batch-service';
import { InventoryBatch } from '../../core/inventory-batch';
import { Auth } from '../../core/auth';
import { Modal } from '../../shared/modal/modal';

type ModalKind = 'receive' | 'transfer' | 'adjust' | 'quarantine' | null;

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [FormsModule, Modal],
  templateUrl: './inventory.html',
  styleUrl: './inventory.scss'
})
export class Inventory implements OnInit {
  productQuery = '';
  productId = '';
  selectedProductName = '';
  suggestions = signal<Product[]>([]);
  showSuggestions = signal(false);
  searching = signal(false);
  private searchTimer?: ReturnType<typeof setTimeout>;

  warehouses = signal<Warehouse[]>([]);
  batches = signal<InventoryBatch[]>([]);
  loadingBatches = signal(false);

  activeModal = signal<ModalKind>(null);
  activeBatch: InventoryBatch | null = null;
  modalError = signal<string | null>(null);
  saving = signal(false);

  receiveForm = { warehouseId: '', batchNumber: '', quantity: 1, expiryDate: '' };
  transferForm = { toWarehouseId: '', quantity: 1 };
  adjustForm = { quantityChange: 0, reason: '' };
  quarantineForm = { reason: '' };

  constructor(
    private productService: ProductService,
    private warehouseService: WarehouseService,
    private batchService: InventoryBatchService,
    public auth: Auth
  ) {}

  ngOnInit(): void {
    this.warehouseService.getAll().subscribe(warehouses => this.warehouses.set(warehouses));
  }

  canReceiveOrTransfer(): boolean {
    return this.auth.hasRole('Administrator', 'InventoryManager', 'WarehouseEmployee');
  }

  canAdjustOrQuarantine(): boolean {
    return this.auth.hasRole('Administrator', 'InventoryManager');
  }

  onProductQueryChange(): void {
    this.productId = '';
    this.showSuggestions.set(true);

    if (this.searchTimer) clearTimeout(this.searchTimer);
    if (!this.productQuery.trim()) {
      this.suggestions.set([]);
      return;
    }

    this.searching.set(true);
    this.searchTimer = setTimeout(() => {
      this.productService.getPaged(1, 8, this.productQuery).subscribe({
        next: (result) => {
          this.suggestions.set(result.items);
          this.searching.set(false);
        },
        error: () => this.searching.set(false)
      });
    }, 250);
  }

  selectProduct(product: Product): void {
    this.productId = product.id;
    this.selectedProductName = product.name;
    this.productQuery = product.name;
    this.showSuggestions.set(false);
    this.loadBatches();
  }

  hideSuggestionsSoon(): void {
    setTimeout(() => this.showSuggestions.set(false), 150);
  }

  loadBatches(): void {
    if (!this.productId) return;
    this.loadingBatches.set(true);
    this.batchService.getByProduct(this.productId).subscribe({
      next: (batches) => {
        this.batches.set(batches);
        this.loadingBatches.set(false);
      },
      error: () => this.loadingBatches.set(false)
    });
  }

  openReceive(): void {
    this.receiveForm = { warehouseId: this.warehouses()[0]?.id ?? '', batchNumber: '', quantity: 1, expiryDate: '' };
    this.modalError.set(null);
    this.activeModal.set('receive');
  }

  openTransfer(batch: InventoryBatch): void {
    this.activeBatch = batch;
    this.transferForm = { toWarehouseId: this.warehouses().find(w => w.id !== batch.warehouseId)?.id ?? '', quantity: 1 };
    this.modalError.set(null);
    this.activeModal.set('transfer');
  }

  openAdjust(batch: InventoryBatch): void {
    this.activeBatch = batch;
    this.adjustForm = { quantityChange: 0, reason: '' };
    this.modalError.set(null);
    this.activeModal.set('adjust');
  }

  openQuarantine(batch: InventoryBatch): void {
    this.activeBatch = batch;
    this.quarantineForm = { reason: '' };
    this.modalError.set(null);
    this.activeModal.set('quarantine');
  }

  closeModal(): void {
    this.activeModal.set(null);
    this.activeBatch = null;
  }

  submitReceive(): void {
    this.modalError.set(null);
    this.saving.set(true);
    this.batchService.receive({
      productId: this.productId,
      warehouseId: this.receiveForm.warehouseId,
      batchNumber: this.receiveForm.batchNumber,
      quantity: Number(this.receiveForm.quantity),
      expiryDate: this.receiveForm.expiryDate
    }).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.loadBatches(); },
      error: (err) => { this.saving.set(false); this.modalError.set(this.extractError(err)); }
    });
  }

  submitTransfer(): void {
    if (!this.activeBatch) return;
    this.modalError.set(null);
    this.saving.set(true);
    this.batchService.transfer({
      inventoryBatchId: this.activeBatch.id,
      toWarehouseId: this.transferForm.toWarehouseId,
      quantity: Number(this.transferForm.quantity)
    }).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.loadBatches(); },
      error: (err) => { this.saving.set(false); this.modalError.set(this.extractError(err)); }
    });
  }

  submitAdjust(): void {
    if (!this.activeBatch) return;
    this.modalError.set(null);
    this.saving.set(true);
    this.batchService.adjust({
      inventoryBatchId: this.activeBatch.id,
      quantityChange: Number(this.adjustForm.quantityChange),
      reason: this.adjustForm.reason
    }).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.loadBatches(); },
      error: (err) => { this.saving.set(false); this.modalError.set(this.extractError(err)); }
    });
  }

  submitQuarantine(): void {
    if (!this.activeBatch) return;
    this.modalError.set(null);
    this.saving.set(true);
    this.batchService.quarantine({
      inventoryBatchId: this.activeBatch.id,
      reason: this.quarantineForm.reason
    }).subscribe({
      next: () => { this.saving.set(false); this.closeModal(); this.loadBatches(); },
      error: (err) => { this.saving.set(false); this.modalError.set(this.extractError(err)); }
    });
  }

  statusBadge(status: string): string {
    switch (status) {
      case 'Available': return 'badge-success';
      case 'Quarantined': return 'badge-danger';
      case 'Expired': return 'badge-warning';
      default: return 'badge-neutral';
    }
  }

  private extractError(err: any): string {
    return err?.error && typeof err.error === 'string' ? err.error : 'Something went wrong. Please try again.';
  }
}
