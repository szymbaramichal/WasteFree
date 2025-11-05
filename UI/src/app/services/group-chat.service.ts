import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from 'environments/environment';
import { Result } from '@app/_models/result';
import { GroupChatConnectionState, GroupChatMessage } from '@app/_models/group-chat';

@Injectable({ providedIn: 'root' })
export class GroupChatService {
  private readonly http = inject(HttpClient);

  private hubConnection?: signalR.HubConnection;
  private connectionPromise?: Promise<void>;
  private currentGroupId: string | null = null;
  private readonly hubCandidates = this.resolveHubUrls();
  private currentHubUrl: string | null = null;

  private readonly messageSubject = new Subject<GroupChatMessage>();
  private readonly connectionStateSubject = new BehaviorSubject<GroupChatConnectionState>('disconnected');

  readonly messages$ = this.messageSubject.asObservable();
  readonly connectionState$ = this.connectionStateSubject.asObservable();

  private readonly apiUrl = `${environment.apiUrl}/garbage-groups`;

  fetchMessages(groupId: string, pageNumber: number, pageSize: number): Observable<Result<GroupChatMessage[]>> {
    const params = new HttpParams()
      .set('pageNumber', String(Math.max(1, pageNumber)))
      .set('pageSize', String(Math.max(1, pageSize)));

    return this.http.get<Result<GroupChatMessage[]>>(
      `${this.apiUrl}/${encodeURIComponent(groupId)}/chat/messages`,
      { params }
    );
  }

  sendMessage(groupId: string, content: string): Observable<Result<GroupChatMessage>> {
    return this.http.post<Result<GroupChatMessage>>(
      `${this.apiUrl}/${encodeURIComponent(groupId)}/chat/messages`,
      { content }
    );
  }

  async joinGroup(groupId: string): Promise<void> {
    if (!groupId) {
      return;
    }

  await this.ensureConnection();

    if (!this.hubConnection) {
      return;
    }

    if (this.currentGroupId === groupId && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.connectionStateSubject.next('connected');
      return;
    }

    if (this.currentGroupId && this.currentGroupId !== groupId && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.leaveCurrentGroup();
    }

    this.connectionStateSubject.next('connecting');

    try {
      await this.hubConnection.invoke('JoinGroup', groupId);
      this.currentGroupId = groupId;
      this.connectionStateSubject.next('connected');
    } catch (error) {
      this.connectionStateSubject.next('error');
      throw error;
    }
  }

  async leaveCurrentGroup(): Promise<void> {
    if (!this.hubConnection || !this.currentGroupId) {
      this.currentGroupId = null;
      return;
    }

    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('LeaveGroup', this.currentGroupId);
      } catch {
        // ignore - connection might already be closed
      }
    }

    this.currentGroupId = null;
    this.connectionStateSubject.next('disconnected');
  }

  async stop(): Promise<void> {
    await this.leaveCurrentGroup();

    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
      } finally {
        this.hubConnection = undefined;
        this.connectionPromise = undefined;
        this.connectionStateSubject.next('disconnected');
      }
    }
  }

  private buildConnection(url: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => localStorage.getItem('authToken') ?? ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .build();

    this.hubConnection.on('ReceiveGroupMessage', (payload: GroupChatMessage) => {
      this.messageSubject.next(payload);
    });

    this.hubConnection.onreconnecting(() => {
      this.connectionStateSubject.next('reconnecting');
    });

    this.hubConnection.onreconnected(async () => {
      if (this.currentGroupId) {
        try {
          await this.hubConnection?.invoke('JoinGroup', this.currentGroupId);
        } catch (error) {
          this.connectionStateSubject.next('error');
          return;
        }
      }

      this.connectionStateSubject.next('connected');
    });

    this.hubConnection.onclose(() => {
      this.currentGroupId = null;
      this.currentHubUrl = null;
      this.connectionStateSubject.next('disconnected');
    });
  }

  private ensureConnection(): Promise<void> {
    if (!this.hubConnection) {
      return this.startConnectionWithFallback();
    }

    if (!this.hubConnection) {
      return Promise.reject(new Error('Unable to initialize SignalR connection.'));
    }

    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return Promise.resolve();
    }

    if (this.hubConnection.state === signalR.HubConnectionState.Connecting ||
        this.hubConnection.state === signalR.HubConnectionState.Reconnecting) {
      return this.connectionPromise ?? Promise.resolve();
    }

    this.connectionStateSubject.next('connecting');

    this.connectionPromise = this.hubConnection.start().catch(async error => {
      this.connectionStateSubject.next('error');
      await this.hubConnection?.stop().catch(() => undefined);
      this.hubConnection = undefined;
      this.connectionPromise = undefined;
      throw error;
    }).finally(() => {
      this.connectionPromise = undefined;
    });

    return this.connectionPromise;
  }

  private async startConnectionWithFallback(): Promise<void> {
    const errors: unknown[] = [];

    for (const candidate of this.hubCandidates) {
      try {
        this.connectionStateSubject.next('connecting');
        this.buildConnection(candidate);
        this.connectionPromise = this.hubConnection?.start();
        await this.connectionPromise;
        this.connectionPromise = undefined;
        this.currentHubUrl = candidate;
        return;
      } catch (error) {
        errors.push(error);
        await this.hubConnection?.stop().catch(() => undefined);
        this.hubConnection = undefined;
        this.connectionPromise = undefined;
      }
    }

    this.connectionStateSubject.next('error');
    const lastError = errors.at(-1) ?? new Error('Unable to establish SignalR connection.');
    throw lastError;
  }

  private resolveHubUrls(): string[] {
    const urls = new Set<string>();

    const addCandidate = (candidate?: string) => {
      if (!candidate) {
        return;
      }

      const cleaned = candidate.replace(/\/+$/, '');
      if (!cleaned) {
        return;
      }

      urls.add(cleaned);
    };

    const apiBase = environment.apiUrl?.trim();
    if (apiBase) {
      const normalized = apiBase.replace(/\/+$/, '');
      addCandidate(`${normalized}/chatHub`);

      if (/\/api$/i.test(normalized)) {
        addCandidate(`${normalized.replace(/\/api$/i, '')}/chatHub`);
      }
    }

    if (typeof window !== 'undefined') {
      const origin = window.location.origin.replace(/\/+$/, '');
      addCandidate(`${origin}/api/chatHub`);
      addCandidate(`${origin}/chatHub`);

      if (/^localhost$|^127\.0\.0\.1$/.test(window.location.hostname)) {
        addCandidate('http://localhost:8080/chatHub');
        addCandidate('https://localhost:8080/chatHub');
        addCandidate('http://127.0.0.1:8080/chatHub');
      }
    } else {
      addCandidate('/api/chatHub');
      addCandidate('/chatHub');
    }

    return Array.from(urls);
  }
}
