import { Component } from '@angular/core';
import { AlertHub } from '../../core/alert-hub';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [],
  templateUrl: './alerts.html',
  styleUrl: './alerts.scss'
})
export class Alerts {
  constructor(public alertHub: AlertHub) {}
}