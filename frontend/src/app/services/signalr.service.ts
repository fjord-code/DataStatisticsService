import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { ReadingSnapshot } from './statistics.service';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private connection?: signalR.HubConnection;
  readonly readingUpdated$ = new Subject<ReadingSnapshot>();

  async start(): Promise<void> {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.notificationUrl)
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReadingUpdated', (payload: ReadingSnapshot) => {
      this.readingUpdated$.next(payload);
    });

    await this.connection.start();
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
    this.connection = undefined;
  }
}
