import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '@app/pipes/translate.pipe';

@Component({
  selector: 'app-garbage-admin-consent-modal',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './garbage-admin-consent-modal.component.html',
  styleUrls: ['./garbage-admin-consent-modal.component.css']
})
export class GarbageAdminConsentModalComponent {
  @Input() visible = false;
  @Input() loading = false;
  @Input() acceptLoading = false;
  @Input() consentText: string | null = null;
  @Input() error: string | null = null;

  @Output() accept = new EventEmitter<void>();
  @Output() reject = new EventEmitter<void>();
  @Output() retry = new EventEmitter<void>();

  onAccept() {
    if (this.acceptLoading || this.loading || this.error) {
      return;
    }
    this.accept.emit();
  }

  onReject() {
    this.reject.emit();
  }

  onRetry() {
    if (this.loading) {
      return;
    }
    this.retry.emit();
  }
}
