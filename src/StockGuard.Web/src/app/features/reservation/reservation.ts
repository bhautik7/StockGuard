import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ReservationService } from '../../core/reservation-service';
import { Reservation } from '../../core/reservation';
import { ProductService } from '../../core/product-service';
import { Product } from '../../core/product';

@Component({
  selector: 'app-reservation',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './reservation.html',
  styleUrl: './reservation.scss'
})
export class ReservationPage implements OnInit {
  productQuery = '';
  productId = '';
  selectedProductName = '';
  quantity = 1;

  suggestions = signal<Product[]>([]);
  showSuggestions = signal(false);
  searching = signal(false);

  result = signal<Reservation | null>(null);
  errorMessage = signal<string | null>(null);
  loading = signal(false);

  private searchTimer?: ReturnType<typeof setTimeout>;

  constructor(
    private reservationService: ReservationService,
    private productService: ProductService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const productId = params.get('productId');
    const productName = params.get('productName');
    if (productId) {
      this.productId = productId;
      this.selectedProductName = productName ?? productId;
      this.productQuery = this.selectedProductName;
    }
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
  }

  hideSuggestionsSoon(): void {
    setTimeout(() => this.showSuggestions.set(false), 150);
  }

  adjustQuantity(delta: number): void {
    this.quantity = Math.max(1, this.quantity + delta);
  }

  onReserve(): void {
    if (!this.productId) {
      this.errorMessage.set('Pick a product from the list before reserving.');
      return;
    }

    this.result.set(null);
    this.errorMessage.set(null);
    this.loading.set(true);

    this.reservationService.reserve(this.productId, this.quantity).subscribe({
      next: (reservation) => {
        this.result.set(reservation);
        this.loading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error ?? 'Reservation failed — insufficient stock or a conflict occurred.');
        this.loading.set(false);
      }
    });
  }
}
