import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@app/pipes/translate.pipe';

@Component({
  selector: 'app-utilization-fee-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './utilization-fee-modal.component.html',
  styleUrls: ['./utilization-fee-modal.component.css']
})
export class UtilizationFeeModalComponent implements OnChanges {
  @Input() visible = false;
  @Input() loading = false;
  @Input() error: string | null = null;

  @Output() submitFee = new EventEmitter<{ amount: number; proof: File }>();
  @Output() dismiss = new EventEmitter<void>();

  amount: number | null = null;
  proof: File | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && changes['visible'].currentValue && !changes['visible'].previousValue) {
      this.reset();
    }
  }

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.proof = input.files && input.files.length > 0 ? input.files[0] : null;
  }

  onSubmit() {
    if (this.amount !== null && this.proof) {
      this.submitFee.emit({ amount: this.amount, proof: this.proof });
    }
  }

  onDismiss() {
    this.dismiss.emit();
  }

  private reset() {
    this.amount = null;
    this.proof = null;
  }
}
