import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CategoryService, CategoryPayload } from '../../core/category-service';
import { Category } from '../../core/category';
import { Auth } from '../../core/auth';
import { Modal } from '../../shared/modal/modal';

const EMPTY_FORM: CategoryPayload = { name: '', description: '' };

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [FormsModule, Modal],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories implements OnInit {
  categories = signal<Category[]>([]);
  loading = signal(false);

  showForm = signal(false);
  editingId = signal<string | null>(null);
  form: CategoryPayload = { ...EMPTY_FORM };
  formError = signal<string | null>(null);
  saving = signal(false);

  constructor(private categoryService: CategoryService, public auth: Auth) {}

  ngOnInit(): void {
    this.load();
  }

  canManage(): boolean {
    return this.auth.hasRole('Administrator', 'InventoryManager');
  }

  load(): void {
    this.loading.set(true);
    this.categoryService.getAll().subscribe({
      next: (categories) => {
        this.categories.set(categories);
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

  openEdit(category: Category): void {
    this.editingId.set(category.id);
    this.form = { name: category.name, description: category.description };
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
    const request = id ? this.categoryService.update(id, this.form) : this.categoryService.create(this.form);

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

  deleteCategory(category: Category): void {
    if (!confirm(`Delete category "${category.name}"?`)) return;

    this.categoryService.delete(category.id).subscribe({
      next: () => this.load(),
      error: (err) => alert(this.extractError(err))
    });
  }

  private extractError(err: any): string {
    return err?.error && typeof err.error === 'string' ? err.error : 'Something went wrong. Please try again.';
  }
}
