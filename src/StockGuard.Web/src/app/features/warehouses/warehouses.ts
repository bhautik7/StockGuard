import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WarehouseService, WarehousePayload } from '../../core/warehouse-service';
import { Warehouse } from '../../core/warehouse';
import { Auth } from '../../core/auth';
import { Modal } from '../../shared/modal/modal';

const EMPTY_FORM: WarehousePayload = { name: '', location: '' };

@Component({
  selector: 'app-warehouses',
  standalone: true,
  imports: [FormsModule, Modal],
  templateUrl: './warehouses.html',
  styleUrl: './warehouses.scss'
})
export class Warehouses implements OnInit {
  warehouses = signal<Warehouse[]>([]);
  loading = signal(false);

  showForm = signal(false);
  editingId = signal<string | null>(null);
  form: WarehousePayload = { ...EMPTY_FORM };
  formError = signal<string | null>(null);
  saving = signal(false);

  constructor(private warehouseService: WarehouseService, public auth: Auth) {}

  ngOnInit(): void {
    this.load();
  }

  canManage(): boolean {
    return this.auth.hasRole('Administrator', 'InventoryManager');
  }

  load(): void {
    this.loading.set(true);
    this.warehouseService.getAll().subscribe({
      next: (warehouses) => {
        this.warehouses.set(warehouses);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form = { ...EMPTY_FORM };
    this.formError.set(null);
    this.showForm.set(true);
  }

  openEdit(warehouse: Warehouse): void {
    this.editingId.set(warehouse.id);
    this.form = { name: warehouse.name, location: warehouse.location };
    this.formError.set(null);
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
  }

  save(): void {
    this.formError.set(null);
    this.saving.set(true);

    const id = this.editingId();
    const request = id ? this.warehouseService.update(id, this.form) : this.warehouseService.create(this.form);

    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        this.formError.set(this.extractError(err));
      }
    });
  }

  deleteWarehouse(warehouse: Warehouse): void {
    if (!confirm(`Delete warehouse "${warehouse.name}"?`)) return;

    this.warehouseService.delete(warehouse.id).subscribe({
      next: () => this.load(),
      error: (err) => alert(this.extractError(err))
    });
  }

  private extractError(err: any): string {
    return err?.error && typeof err.error === 'string' ? err.error : 'Something went wrong. Please try again.';
  }
}
