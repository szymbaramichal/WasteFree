import { CommonModule, DatePipe } from '@angular/common';
import { Component, ElementRef, Input, OnChanges, OnDestroy, SimpleChanges, ViewChild, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subscription, firstValueFrom } from 'rxjs';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { GroupChatService } from '@app/services/group-chat.service';
import { GroupChatConnectionState, GroupChatMessage } from '@app/_models/group-chat';
import { CurrentUserService } from '@app/services/current-user.service';

@Component({
  selector: 'app-group-chat',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe, DatePipe],
  templateUrl: './group-chat.component.html',
  styleUrls: ['./group-chat.component.css']
})
export class GroupChatComponent implements OnChanges, OnDestroy {
  @Input() groupId: string | null = null;
  @Input() groupName: string | null = null;
  @Input() active = false;

  @ViewChild('messagesContainer', { static: false }) messagesContainer?: ElementRef<HTMLDivElement>;

  private readonly chatService = inject(GroupChatService);
  private readonly fb = inject(FormBuilder);
  private readonly currentUserService = inject(CurrentUserService);

  private readonly subscriptions: Subscription[] = [];
  private messageSubscription?: Subscription;

  messages: GroupChatMessage[] = [];
  private readonly messageIds = new Set<string>();

  loading = false;
  loadingOlder = false;
  sending = false;
  error: string | null = null;
  sendError: string | null = null;

  connectionState: GroupChatConnectionState = 'disconnected';
  pageNumber = 1;
  readonly pageSize = 30;
  hasMore = false;
  private initializedGroupId: string | null = null;
  private readonly currentUserId: string | null = this.currentUserService.user()?.id ?? null;

  readonly messageForm = this.fb.group({
    message: ['', [Validators.required, Validators.maxLength(2000)]]
  });

  constructor() {
    this.subscriptions.push(
      this.chatService.connectionState$.subscribe(state => {
        this.connectionState = state;
        if (state === 'connected') {
          this.scrollToBottomDeferred();
        }
      })
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['groupId'] && !changes['groupId'].firstChange) {
      this.resetHistory();
      this.unsubscribeFromMessages();
      this.initializedGroupId = null;
      if (this.active) {
        void this.activateChat();
      }
    }

    if (changes['active']) {
      if (this.active) {
        void this.activateChat();
      } else {
        void this.deactivateChat();
      }
    }
  }

  ngOnDestroy(): void {
    this.cleanupSubscriptions();
    void this.chatService.stop();
  }

  async loadMore(): Promise<void> {
    if (!this.groupId || this.loadingOlder || !this.hasMore) {
      return;
    }

    this.loadingOlder = true;
    try {
      const nextPage = this.pageNumber + 1;
      const loaded = await this.loadMessages(nextPage, true);
      if (loaded) {
        this.pageNumber = nextPage;
      }
    } finally {
      this.loadingOlder = false;
    }
  }

  async send(): Promise<void> {
    if (!this.groupId || this.sending) {
      return;
    }

    this.sendError = null;

    if (this.messageForm.invalid) {
      this.messageForm.markAllAsTouched();
      return;
    }

    const raw = this.messageForm.value.message ?? '';
    const content = raw.trim();

    if (!content) {
      this.messageForm.controls.message.setValue('');
      return;
    }

    this.sending = true;
    try {
      const result = await firstValueFrom(this.chatService.sendMessage(this.groupId, content));
      if (result && !result.errorMessage) {
        const newMessage = result.resultModel;
        if (newMessage) {
          this.mergeMessages([newMessage], false);
        }

        this.messageForm.reset();
        this.scrollToBottomDeferred();
      } else if (result?.errorMessage) {
        this.sendError = result.errorMessage;
      }
    } catch (error: any) {
      this.sendError = error?.error?.errorMessage || error?.message || 'Unable to send message.';
    } finally {
      this.sending = false;
    }
  }

  onComposerEnter(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;

    if (keyboardEvent.defaultPrevented || keyboardEvent.shiftKey || keyboardEvent.altKey || keyboardEvent.ctrlKey || keyboardEvent.metaKey || (keyboardEvent as any).isComposing) {
      return;
    }

    if (keyboardEvent.key !== 'Enter') {
      return;
    }

    keyboardEvent.preventDefault();
    void this.send();
  }

  isMine(message: GroupChatMessage): boolean {
    return !!this.currentUserId && message.userId === this.currentUserId;
  }

  trackByMessageId(index: number, message: GroupChatMessage): string {
    return message.id;
  }

  connectionBadgeClass(): string {
    switch (this.connectionState) {
      case 'connected':
        return 'bg-success-subtle text-success border-success-subtle';
      case 'connecting':
        return 'bg-warning-subtle text-warning border-warning-subtle';
      case 'reconnecting':
        return 'bg-info-subtle text-info border-info-subtle';
      case 'error':
        return 'bg-danger-subtle text-danger border-danger-subtle';
      default:
        return 'bg-secondary-subtle text-secondary border-secondary-subtle';
    }
  }

  private async activateChat(): Promise<void> {
    if (!this.groupId) {
      return;
    }

    this.error = null;

    if (this.initializedGroupId !== this.groupId) {
      this.resetHistory();
      this.initializedGroupId = this.groupId;
  await this.loadMessages(1, false);
    }

    this.subscribeToMessages();

    try {
      await this.chatService.joinGroup(this.groupId);
    } catch (error: any) {
      this.error = error?.error?.errorMessage || error?.message || 'Unable to connect to chat.';
    }
  }

  private async deactivateChat(): Promise<void> {
    await this.chatService.leaveCurrentGroup();
    this.unsubscribeFromMessages();
  }

  private async loadMessages(page: number, prepend: boolean): Promise<boolean> {
    if (!this.groupId) {
      return false;
    }

    if (page === 1) {
      this.loading = true;
    }

    try {
      const result = await firstValueFrom(this.chatService.fetchMessages(this.groupId, page, this.pageSize));

      if (result?.errorMessage) {
        this.error = result.errorMessage;
        return false;
      }

      const payload = result?.resultModel ?? [];
      this.mergeMessages(payload, prepend);

      if (result?.pager) {
        this.hasMore = result.pager.totalPages > page;
      } else {
        this.hasMore = payload.length === this.pageSize;
      }

      if (!prepend) {
        this.pageNumber = 1;
        this.scrollToBottomDeferred();
      }

      return true;
    } catch (error: any) {
      this.error = error?.error?.errorMessage || error?.message || 'Unable to load chat history.';
      return false;
    } finally {
      if (page === 1) {
        this.loading = false;
      }
    }

    return false;
  }

  private mergeMessages(messages: GroupChatMessage[], prepend: boolean): void {
    if (!messages.length) {
      return;
    }

    const sorted = [...messages].sort((a, b) =>
      new Date(a.sentAtUtc).getTime() - new Date(b.sentAtUtc).getTime()
    );

    for (const incoming of sorted) {
      if (this.messageIds.has(incoming.id)) {
        continue;
      }

      const normalized = this.normalizeMessage(incoming);

      this.messageIds.add(normalized.id);
      if (prepend) {
        this.messages.unshift(normalized);
      } else {
        this.messages.push(normalized);
      }
    }

    this.messages.sort((a, b) => new Date(a.sentAtUtc).getTime() - new Date(b.sentAtUtc).getTime());
  }

  private subscribeToMessages(): void {
    if (this.messageSubscription) {
      return;
    }

    this.messageSubscription = this.chatService.messages$.subscribe(message => {
      if (!this.groupId || message.groupId !== this.groupId) {
        return;
      }

      if (this.messageIds.has(message.id)) {
        return;
      }

      const normalized = this.normalizeMessage(message);

      this.messageIds.add(normalized.id);
      this.messages.push(normalized);
      this.messages.sort((a, b) => new Date(a.sentAtUtc).getTime() - new Date(b.sentAtUtc).getTime());
      this.scrollToBottomDeferred();
    });
  }

  private unsubscribeFromMessages(): void {
    this.messageSubscription?.unsubscribe();
    this.messageSubscription = undefined;
  }

  private cleanupSubscriptions(): void {
    this.unsubscribeFromMessages();
    while (this.subscriptions.length) {
      this.subscriptions.pop()?.unsubscribe();
    }
  }

  private resetHistory(): void {
    this.messages = [];
    this.messageIds.clear();
    this.pageNumber = 1;
    this.hasMore = false;
    this.loading = false;
    this.loadingOlder = false;
    this.sendError = null;
  }

  private scrollToBottomDeferred(): void {
    if (!this.active) {
      return;
    }

    setTimeout(() => {
      const container = this.messagesContainer?.nativeElement;
      if (!container) {
        return;
      }

      const distanceFromBottom = container.scrollHeight - (container.scrollTop + container.clientHeight);
      if (distanceFromBottom > 150) {
        return;
      }

      container.scrollTop = container.scrollHeight;
    }, 120);
  }

  private normalizeMessage(message: GroupChatMessage): GroupChatMessage {
    const sentAtUtc = this.normalizeUtcTimestamp(message.sentAtUtc);
    if (sentAtUtc === message.sentAtUtc) {
      return message;
    }

    return {
      ...message,
      sentAtUtc
    };
  }

  private normalizeUtcTimestamp(input: string): string {
    const raw = input?.trim();
    if (!raw) {
      return input;
    }

    if (/[+-]\d{2}:\d{2}$|Z$/i.test(raw)) {
      const parsed = Date.parse(raw);
      return Number.isNaN(parsed) ? raw : new Date(parsed).toISOString();
    }

    const appended = `${raw}Z`;
    const parsedAppended = Date.parse(appended);
    if (!Number.isNaN(parsedAppended)) {
      return new Date(parsedAppended).toISOString();
    }

    const fallback = Date.parse(raw);
    return Number.isNaN(fallback) ? raw : new Date(fallback).toISOString();
  }
}
