import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SupplierService, SupplierPayload } from '../../core/supplier-service';
import { Supplier } from '../../core/supplier';
import { Auth } from '../../core/auth';
import { Modal } from '../../shared/modal/modal';

const EMPTY_FORM: SupplierPayload = { name: '', contactEmail: '', contactPhone: '' };

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [FormsModule, Modal],
  templateUrl: './suppliers.html',
  styleUrl: './suppliers.scss'
})
export class Suppliers implements OnInit {
  suppliers = signal<Supplier[]>([]);
  loading = signal(false);

  showForm = signal(false);
  editingId = signal<string | null>(null);
  form: SupplierPayload = { ...EMPTY_FORM };
  formError = signal<string | null>(null);
  saving = signal(false);

  constructor(private supplierService: SupplierService, public auth: Auth) {}

  ngOnInit(): void {
    this.load();
  }

  canManage(): boolean {
    return this.auth.hasRole('Administrator', 'PurchasingOfficer');
  }

  load(): void {
    this.loading.set(true);
    this.supplierService.getAll().subscribe({
      next: (suppliers) => {
        this.suppliers.set(suppliers);
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

  openEdit(supplier: Supplier): void {
    this.editingId.set(supplier.id);
    this.form = { name: supplier.name, contactEmail: supplier.contactEmail, contactPhone: supplier.contactPhone };
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
    const request = id ? this.supplierService.update(id, this.form) : this.supplierService.create(this.form);

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

  deleteSupplier(supplier: Supplier): void {
    if (!confirm(`Delete supplier "${supplier.name}"?`)) return;

    this.supplierService.delete(supplier.id).subscribe({
      next: () => this.load(),
      error: (err) => alert(this.extractError(err))
    });
  }

  private extractError(err: any): string {
    return err?.error && typeof err.error === 'string' ? err.error : 'Something went wrong. Please try again.';
  }
}
