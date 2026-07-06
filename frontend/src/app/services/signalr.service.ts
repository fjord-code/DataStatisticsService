import { Injectable, NgZone, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { ReadingSnapshot } from './statistics.service';

export type SignalRConnectionState = 'disconnected' | 'connecting' | 'connected' | 'failed';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly ngZone = inject(NgZone);
  private connection?: signalR.HubConnection;

  readonly readingUpdated$ = new Subject<ReadingSnapshot>();
  readonly connectionState$ = new BehaviorSubject<SignalRConnectionState>('disconnected');

  async start(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    if (this.connection) {
      return;
    }

    this.setConnectionState('connecting');

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.notificationUrl)
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReadingUpdated', (payload: ReadingSnapshot) => {
      this.ngZone.run(() => {
        this.readingUpdated$.next(payload);
      });
    });

    this.connection.onclose(() => {
      this.ngZone.run(() => {
        if (this.connectionState$.value !== 'failed') {
          this.setConnectionState('disconnected');
        }
      });
    });

    this.connection.onreconnecting(() => {
      this.ngZone.run(() => {
        this.setConnectionState('connecting');
      });
    });

    this.connection.onreconnected(() => {
      this.ngZone.run(() => {
        this.setConnectionState('connected');
      });
    });

    try {
      await this.connection.start();
      this.setConnectionState('connected');
    } catch (error) {
      console.error('SignalR connection failed:', error);
      this.setConnectionState('failed');

      try {
        await this.connection.stop();
      } catch {
        // ignore cleanup errors
      }

      this.connection = undefined;
      throw error;
    }
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
    this.connection = undefined;
    this.setConnectionState('disconnected');
  }

  private setConnectionState(state: SignalRConnectionState): void {
    this.connectionState$.next(state);
  }
}
