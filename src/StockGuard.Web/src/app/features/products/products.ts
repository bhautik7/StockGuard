import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../core/product-service';
import { Product } from '../../core/product';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './products.html',
  styleUrl: './products.scss'
})
export class Products implements OnInit {
  products = signal<Product[]>([]);
  totalCount = signal(0);
  page = signal(1);
  pageSize = 20;
  search = '';

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.productService.getPaged(this.page(), this.pageSize, this.search).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalCount.set(result.totalCount);
      }
    });
  }

  onSearch(): void {
    this.page.set(1);
    this.loadProducts();
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
}