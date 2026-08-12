import { Component } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AlertHub } from '../../core/alert-hub';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './alerts.html',
  styleUrl: './alerts.scss'
})
export class Alerts {
  constructor(public alertHub: AlertHub) {}

  severity(message: string): 'danger' | 'warning' | 'info' {
    const lower = message.toLowerCase();
    if (lower.includes('quarantine')) return 'danger';
    if (lower.includes('expire') || lower.includes('reorder')) return 'warning';
    return 'info';
  }

  label(message: string): string {
    switch (this.severity(message)) {
      case 'danger':
        return 'Critical';
      case 'warning':
        return 'Warning';
      default:
        return 'Notice';
    }
  }
}
