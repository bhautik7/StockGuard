import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class AlertHub {
  liveMessages = signal<string[]>([]);
  private connection: signalR.HubConnection;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5270/hubs/alerts')
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceiveAlert', (message: string) => {
      this.liveMessages.update(msgs => [message, ...msgs]);
    });

    this.connection.start();
  }
}