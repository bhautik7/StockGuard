import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProductService, ProductPayload } from '../../core/product-service';
import { Product } from '../../core/product';
import { CategoryService } from '../../core/category-service';
import { Category } from '../../core/category';
import { Auth } from '../../core/auth';
import { Modal } from '../../shared/modal/modal';

const EMPTY_FORM: ProductPayload = { sku: '', name: '', description: '', unit: 'each', reorderLevel: 0, categoryId: '' };

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [FormsModule, RouterLink, Modal],
  templateUrl: './products.html',
  styleUrl: './products.scss'
})
export class Products implements OnInit {
  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  page = signal(1);
  pageSize = 20;
  search = '';
  categoryId = '';

  showForm = signal(false);
  editingId = signal<string | null>(null);
  form: ProductPayload = { ...EMPTY_FORM };
  formError = signal<string | null>(null);
  saving = signal(false);

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    public auth: Auth
  ) {}

  ngOnInit(): void {
    this.categoryService.getAll().subscribe(categories => this.categories.set(categories));
    this.loadProducts();
  }

  canManage(): boolean {
    return this.auth.hasRole('Administrator', 'InventoryManager');
  }

  loadProducts(): void {
    this.loading.set(true);
    this.productService.getPaged(this.page(), this.pageSize, this.search, this.categoryId || undefined).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearch(): void {
    this.page.set(1);
    this.loadProducts();
  }

  clearFilters(): void {
    this.search = '';
    this.categoryId = '';
    this.onSearch();
  }

  nextPage(): void {
    this.page.update(p => p + 1);
    this.loadProducts();
  }

  prevPage(): void {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
      this.loadProducts();
    }
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form = { ...EMPTY_FORM, categoryId: this.categories()[0]?.id ?? '' };
    this.formError.set(null);
    this.showForm.set(true);
  }

  openEdit(product: Product): void {
    this.editingId.set(product.id);
    this.form = {
      sku: product.sku,
      name: product.name,
      description: product.description,
      unit: product.unit,
      reorderLevel: product.reorderLevel,
      categoryId: product.categoryId
    };
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
    const request = id ? this.productService.update(id, this.form) : this.productService.create(this.form);

    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.loadProducts();
      },
      error: (err) => {
        this.saving.set(false);
        this.formError.set(this.extractError(err));
      }
    });
  }

  deleteProduct(product: Product): void {
    if (!confirm(`Delete "${product.name}"? This cannot be undone.`)) return;

    this.productService.delete(product.id).subscribe({
      next: () => this.loadProducts(),
      error: (err) => alert(this.extractError(err))
    });
  }

  private extractError(err: any): string {
    return err?.error && typeof err.error === 'string' ? err.error : 'Something went wrong. Please try again.';
  }
}
