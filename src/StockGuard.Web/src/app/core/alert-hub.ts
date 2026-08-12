import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';

export type ConnectionStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

export interface AlertEvent {
  message: string;
  receivedAt: Date;
}

@Injectable({ providedIn: 'root' })
export class AlertHub {
  liveMessages = signal<AlertEvent[]>([]);
  status = signal<ConnectionStatus>('connecting');
  private connection: signalR.HubConnection;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5270/hubs/alerts')
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceiveAlert', (message: string) => {
      this.liveMessages.update(msgs => [{ message, receivedAt: new Date() }, ...msgs]);
    });

    this.connection.onreconnecting(() => this.status.set('reconnecting'));
    this.connection.onreconnected(() => this.status.set('connected'));
    this.connection.onclose(() => this.status.set('disconnected'));

    this.connection
      .start()
      .then(() => this.status.set('connected'))
      .catch(() => this.status.set('disconnected'));
  }
}
