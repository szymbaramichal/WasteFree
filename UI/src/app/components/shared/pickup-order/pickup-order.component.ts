import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

type ServiceOption = {
  value: number;
  titleKey: string;
  descriptionKey: string;
  icon: string;
};

type StepConfig = {
  labelKey: string;
  headerLeadKey: string;
  placeholderTitleKey?: string;
  placeholderLeadKey?: string;
  placeholderBodyKey?: string;
};

@Component({
  selector: 'app-pickup-order',
  standalone: true,
  imports: [CommonModule, TranslatePipe, ReactiveFormsModule],
  templateUrl: './pickup-order.component.html',
  styleUrls: ['./pickup-order.component.css']
})
export class PickupOrderComponent {
  private readonly fb = inject(FormBuilder);

  readonly serviceOptions: ServiceOption[] = [
    {
      value: 0,
      titleKey: 'pickupOrder.options.small.title',
      descriptionKey: 'pickupOrder.options.small.description',
      icon: '/assets/images/pickup-small.svg'
    },
    {
      value: 1,
      titleKey: 'pickupOrder.options.regular.title',
      descriptionKey: 'pickupOrder.options.regular.description',
      icon: '/assets/images/pickup-regular.svg'
    },
    {
      value: 2,
      titleKey: 'pickupOrder.options.container.title',
      descriptionKey: 'pickupOrder.options.container.description',
      icon: '/assets/images/pickup-container.svg'
    },
    {
      value: 3,
      titleKey: 'pickupOrder.options.special.title',
      descriptionKey: 'pickupOrder.options.special.description',
      icon: '/assets/images/pickup-special.svg'
    }
  ];

  readonly steps: StepConfig[] = [
    {
      labelKey: 'pickupOrder.steps.orderType',
      headerLeadKey: 'pickupOrder.subtitle'
    },
    {
      labelKey: 'pickupOrder.steps.participants',
      headerLeadKey: 'pickupOrder.step.participants.lead',
      placeholderTitleKey: 'pickupOrder.step.participants.title',
      placeholderLeadKey: 'pickupOrder.step.participants.lead',
      placeholderBodyKey: 'pickupOrder.step.participants.placeholder'
    },
    {
      labelKey: 'pickupOrder.steps.address',
      headerLeadKey: 'pickupOrder.step.address.lead',
      placeholderTitleKey: 'pickupOrder.step.address.title',
      placeholderLeadKey: 'pickupOrder.step.address.lead',
      placeholderBodyKey: 'pickupOrder.step.address.placeholder'
    },
    {
      labelKey: 'pickupOrder.steps.details',
      headerLeadKey: 'pickupOrder.step.details.lead',
      placeholderTitleKey: 'pickupOrder.step.details.title',
      placeholderLeadKey: 'pickupOrder.step.details.lead',
      placeholderBodyKey: 'pickupOrder.step.details.placeholder'
    },
    {
      labelKey: 'pickupOrder.steps.summary',
      headerLeadKey: 'pickupOrder.step.summary.lead',
      placeholderTitleKey: 'pickupOrder.step.summary.title',
      placeholderLeadKey: 'pickupOrder.step.summary.lead',
      placeholderBodyKey: 'pickupOrder.step.summary.placeholder'
    }
  ];

  readonly currentStepConfig = computed<StepConfig>(() => this.steps[this.currentStep()] ?? this.steps[0]);

  readonly form = this.fb.group({
    serviceType: this.fb.control<number | null>(null, { validators: Validators.required })
  });

  readonly currentStep = signal(0);

  readonly selectedOption = computed(() => {
    const current = this.serviceTypeCtrl.value;
    return this.serviceOptions.find(option => option.value === current) ?? null;
  });

  readonly primaryActionDisabled = computed(() => {
    const step = this.currentStep();
    if (step === 0) {
      return this.serviceTypeCtrl.invalid;
    }
    return step >= this.steps.length - 1;
  });

  get serviceTypeCtrl() {
    return this.form.controls.serviceType;
  }

  onNext() {
    const step = this.currentStep();
    if (step === 0) {
      this.serviceTypeCtrl.markAsTouched();
      this.serviceTypeCtrl.markAsDirty();
      if (this.serviceTypeCtrl.invalid) {
        return;
      }
    }

    if (step < this.steps.length - 1) {
      this.currentStep.update(value => value + 1);
    }
  }

  onBack() {
    const step = this.currentStep();
    if (step > 0) {
      this.currentStep.update(value => value - 1);
    }
  }

  trackOptionByValue(_index: number, option: ServiceOption) {
    return option.value;
  }
}
