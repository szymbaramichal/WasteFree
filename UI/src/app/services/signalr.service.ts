import { inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from 'environments/environment';
import { InboxService } from './inbox.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection!: signalR.HubConnection;
  private inboxService = inject(InboxService);
  notifications: string[] = [];
  private readonly hubUrl = this.resolveHubUrl();

  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => localStorage.getItem('authToken') ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('UpdateInboxCounter', (message: string) => {
      this.inboxService.notifyPush();
      const count = parseInt(message, 10);
      if (!isNaN(count)) {
        this.inboxService.counter.set(count);
      } else {
        console.warn('Invalid counter value from SignalR:', message);
      }
      this.notifications.push(message);
    });

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('Error while connecting SignalR:', err));
  }

  private resolveHubUrl(): string {
    const base = environment.apiUrl.replace(/\/+$/, '');
    return `${base}/notificationHub`;
  }
}
