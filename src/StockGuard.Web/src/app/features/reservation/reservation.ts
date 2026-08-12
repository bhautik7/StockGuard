import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReservationService } from '../../core/reservation-service';
import { Reservation } from '../../core/reservation';

@Component({
  selector: 'app-reservation',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './reservation.html',
  styleUrl: './reservation.scss'
})
export class ReservationPage {
  productId = '';
  quantity = 1;
  result = signal<Reservation | null>(null);
  errorMessage = signal<string | null>(null);
  loading = signal(false);

  constructor(private reservationService: ReservationService) {}

  onReserve(): void {
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