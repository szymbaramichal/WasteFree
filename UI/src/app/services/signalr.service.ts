import { Injectable, effect, inject } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { CurrentUserService } from './current-user.service';
import { InboxService } from './inbox.service';

const INBOX_COUNTER_METHOD = 'UpdateInboxCounter';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private readonly hubUrl = this.resolveHubUrl();

  private readonly currentUser = inject(CurrentUserService);
  private readonly inbox = inject(InboxService);

  private readonly watcher = effect(() => {
    const user = this.currentUser.user();
    const token = this.getToken();

    if (user && token) {
      this.startConnection();
    } else {
      this.stopConnection();
    }
  });

  private readonly handleInboxCounter = (payload: unknown) => {
    const parsed = this.parseCounter(payload);
    if (parsed !== null) {
      this.inbox.setCounter(parsed);
    }
  };

  private resolveHubUrl(): string {
    const trimmed = environment.apiUrl.replace(/\/+$/, '');
    const withoutApi = trimmed.replace(/\/api$/i, '');
    return `${withoutApi}/notificationHub`;
  }

  private getToken(): string | null {
    try {
      return localStorage.getItem('authToken');
    } catch {
      return null;
    }
  }

  private startConnection(): void {
    const token = this.getToken();
    if (!token) {
      return;
    }

    if (this.hubConnection) {
      const state = this.hubConnection.state;
      if (state === HubConnectionState.Connected || state === HubConnectionState.Connecting) {
        return;
      }
    }

    if (this.hubConnection) {
      this.hubConnection.off(INBOX_COUNTER_METHOD, this.handleInboxCounter);
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.getToken() ?? ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.hubConnection.on(INBOX_COUNTER_METHOD, this.handleInboxCounter);

    this.hubConnection.onreconnected(() => {
      this.inbox.refreshCounter();
    });

    this.hubConnection
      .start()
      .then(() => this.inbox.refreshCounter())
  .catch((err: unknown) => console.error('SignalR connection start failed', err));
  }

  private stopConnection(): void {
    if (!this.hubConnection) {
      return;
    }

    const connection = this.hubConnection;
    this.hubConnection = null;

  connection.off(INBOX_COUNTER_METHOD, this.handleInboxCounter);

    connection
      .stop()
    .catch((err: unknown) => console.warn('SignalR connection stop failed', err));
  }

  private parseCounter(payload: unknown): number | null {
    const value = typeof payload === 'string' ? Number.parseInt(payload, 10) : Number(payload);
    if (Number.isNaN(value) || value < 0) {
      return null;
    }
    return value;
  }
}
