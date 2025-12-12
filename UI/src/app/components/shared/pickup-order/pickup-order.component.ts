import { Component, OnInit, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { LocalizedCityPipe } from '@app/pipes/localized-city.pipe';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { DateAdapter, MAT_DATE_FORMATS, MatNativeDateModule, NativeDateAdapter } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { GarbageGroupService } from '@app/services/garbage-group.service';
import { GarbageGroupWithUsers, GarbageGroupRole } from '@app/_models/garbageGroups';
import {
  CalculateGarbageOrderRequest,
  ContainerSize,
  CreateGarbageOrderRequest,
  GarbageOrderDto,
  PickupOption
} from '@app/_models/garbage-orders';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { Result } from '@app/_models/result';
import { GarbageOrderService } from '@app/services/garbage-order.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TranslationService } from '@app/services/translation.service';
import { timer } from 'rxjs';

type ServiceOption = {
  value: PickupOption;
  titleKey: string;
  descriptionKey: string;
  icon: string;
  disabled?: boolean;
};

type StepConfig = {
  labelKey: string;
  headerLeadKey: string;
  placeholderTitleKey?: string;
  placeholderLeadKey?: string;
  placeholderBodyKey?: string;
};

type GroupsWithUsersViewModel = {
  id: string;
  name: string | null;
  users: GroupUserViewModel[];
  isPrivate: boolean;
  address: GarbageGroupWithUsers['address'];
};

type GroupUserViewModel = {
  id: string;
  username: string;
  disabled: boolean;
  role: GarbageGroupRole;
};

type PickupSchedule = {
  isoString: string;
  displayDate: Date;
};

type TimeOption = {
  value: string;
  label: string;
};

type ContainerSizeOption = {
  value: ContainerSize;
  labelKey: string;
  descriptionKey: string;
};

const PICKUP_ORDER_DATE_FORMATS = {
  parse: {
    dateInput: 'input'
  },
  display: {
    dateInput: 'input',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY'
  }
};

class PickupOrderDateAdapter extends NativeDateAdapter {
  override parse(value: unknown): Date | null {
    if (typeof value === 'string' && this.locale?.toLowerCase().startsWith('pl')) {
      const trimmed = value.trim();
      if (!trimmed) {
        return null;
      }

      const normalized = trimmed.replace(/[\/\-]/g, '.');
      const parts = normalized.split('.');
      if (parts.length === 3) {
        const [dayStr, monthStr, yearStr] = parts;
        const day = Number(dayStr);
        const month = Number(monthStr) - 1;
        const year = Number(yearStr);
        if (!Number.isNaN(day) && !Number.isNaN(month) && !Number.isNaN(year)) {
          const candidate = new Date(year, month, day);
          if (candidate.getFullYear() === year && candidate.getMonth() === month && candidate.getDate() === day) {
            return candidate;
          }
        }
      }
    }

    return super.parse(value);
  }

  override format(date: Date, displayFormat: any): string {
    const locale = (this.locale || '').toLowerCase();
    if (displayFormat === 'input' && locale.startsWith('pl')) {
      const day = this.to2Digit(date.getDate());
      const month = this.to2Digit(date.getMonth() + 1);
      const year = date.getFullYear();
      return `${day}.${month}.${year}`;
    }

    return super.format(date, displayFormat);
  }

  private to2Digit(value: number): string {
    return value < 10 ? `0${value}` : `${value}`;
  }
}

@Component({
  selector: 'app-pickup-order',
  standalone: true,
  imports: [
    CommonModule,
    TranslatePipe,
    LocalizedCityPipe,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule
  ],
  templateUrl: './pickup-order.component.html',
  styleUrls: ['./pickup-order.component.css'],
  providers: [
    { provide: DateAdapter, useClass: PickupOrderDateAdapter },
    { provide: MAT_DATE_FORMATS, useValue: PICKUP_ORDER_DATE_FORMATS }
  ]
})
export class PickupOrderComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly groupService = inject(GarbageGroupService);
  private readonly garbageOrderService = inject(GarbageOrderService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly toastr = inject(ToastrService);
  private readonly translation = inject(TranslationService);
  private readonly dateAdapter = inject(DateAdapter<Date>);
  private lastSelectedContainerSize: ContainerSize = ContainerSize.ContainerSmall;
  private readonly navigationDelayMs = 250;

  readonly serviceOptions: ServiceOption[] = [
    {
      value: PickupOption.SmallPickup,
      titleKey: 'pickupOrder.options.small.title',
      descriptionKey: 'pickupOrder.options.small.description',
      icon: '/assets/images/pickup-small.svg'
    },
    {
      value: PickupOption.Pickup,
      titleKey: 'pickupOrder.options.regular.title',
      descriptionKey: 'pickupOrder.options.regular.description',
      icon: '/assets/images/pickup-regular.svg'
    },
    {
      value: PickupOption.Container,
      titleKey: 'pickupOrder.options.container.title',
      descriptionKey: 'pickupOrder.options.container.description',
      icon: '/assets/images/pickup-container.svg'
    },
    {
      value: PickupOption.SpecialOrder,
      titleKey: 'pickupOrder.options.special.title',
      descriptionKey: 'pickupOrder.options.special.description',
      icon: '/assets/images/pickup-special.svg',
      disabled: true
    }
  ];

  readonly containerSizeOptions: ContainerSizeOption[] = [
    {
      value: ContainerSize.ContainerSmall,
      labelKey: 'pickupOrder.step.details.container.size.small',
      descriptionKey: 'pickupOrder.step.details.container.size.small.description'
    },
    {
      value: ContainerSize.ContainerMedium,
      labelKey: 'pickupOrder.step.details.container.size.medium',
      descriptionKey: 'pickupOrder.step.details.container.size.medium.description'
    },
    {
      value: ContainerSize.ContainerLarge,
      labelKey: 'pickupOrder.step.details.container.size.large',
      descriptionKey: 'pickupOrder.step.details.container.size.large.description'
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

  readonly summaryStepIndex = this.steps.length - 1;

  readonly timeOptions: TimeOption[] = this.generateTimeOptions();

  readonly currentStepConfig = computed<StepConfig>(() => this.steps[this.currentStep()] ?? this.steps[0]);

  readonly form = this.fb.group(
    {
      serviceType: this.fb.control<PickupOption | null>(null, { validators: Validators.required }),
      groupId: this.fb.control<string | null>(null, { validators: Validators.required }),
      participantIds: this.fb.control<string[]>([], { validators: Validators.required }),
      pickupDate: this.fb.control<Date | null>(null, { validators: Validators.required }),
      pickupTime: this.fb.control<string | Date | null>(null, { validators: Validators.required }),
      dropOffDate: this.fb.control<Date | null>(null),
      dropOffTime: this.fb.control<string | Date | null>(null),
      isHighPriority: this.fb.control<boolean>(false),
      collectingService: this.fb.control<boolean>(false),
      containerSize: this.fb.control<ContainerSize | null>(null)
    },
    {
      validators: (control) => this.validateForm(control)
    }
  );

  private readonly groupIdValue = toSignal(this.groupIdCtrl.valueChanges, {
    initialValue: this.groupIdCtrl.value
  });

  private readonly participantIdsValue = toSignal(this.participantIdsCtrl.valueChanges, {
    initialValue: this.participantIdsCtrl.value ?? []
  });

  private readonly serviceTypeValue = toSignal(this.serviceTypeCtrl.valueChanges, {
    initialValue: this.serviceTypeCtrl.value
  });

  private readonly pickupDateValue = toSignal(this.pickupDateCtrl.valueChanges, {
    initialValue: this.pickupDateCtrl.value
  });

  private readonly pickupTimeValue = toSignal(this.pickupTimeCtrl.valueChanges, {
    initialValue: this.pickupTimeCtrl.value
  });

  private readonly dropOffDateValue = toSignal(this.dropOffDateCtrl.valueChanges, {
    initialValue: this.dropOffDateCtrl.value
  });

  private readonly dropOffTimeValue = toSignal(this.dropOffTimeCtrl.valueChanges, {
    initialValue: this.dropOffTimeCtrl.value
  });

  private readonly isHighPriorityValue = toSignal(this.isHighPriorityCtrl.valueChanges, {
    initialValue: this.isHighPriorityCtrl.value ?? false
  });

  private readonly collectingServiceValue = toSignal(this.collectingServiceCtrl.valueChanges, {
    initialValue: this.collectingServiceCtrl.value ?? false
  });

  private readonly containerSizeValue = toSignal(this.containerSizeCtrl.valueChanges, {
    initialValue: this.containerSizeCtrl.value
  });

  readonly currentStep = signal(0);
  readonly loadingGroups = signal(false);
  readonly groups = signal<GroupsWithUsersViewModel[]>([]);
  readonly individualGroup = signal<GroupsWithUsersViewModel | null>(null);
  readonly selectedGroupId = computed(() => this.groupIdValue() ?? null);
  readonly selectedGroup = computed<GroupsWithUsersViewModel | null>(() => {
    const selectedId = this.selectedGroupId();
    if (!selectedId) {
      return null;
    }

    const group = this.groups().find(item => item.id === selectedId);
    if (group) {
      return group;
    }

    const individual = this.individualGroup();
    if (individual && individual.id === selectedId) {
      return individual;
    }

    return null;
  });
  readonly selectedGroupUsers = computed<GroupUserViewModel[]>(() => this.selectedGroup()?.users ?? []);
  readonly hasSelectableParticipants = computed(() => this.selectedGroupUsers().some(user => !user.disabled));

  readonly selectedOption = computed(() => {
    const current = this.serviceTypeValue();
    return this.serviceOptions.find(option => option.value === current) ?? null;
  });

  readonly selectedParticipants = computed<GroupUserViewModel[]>(() => {
    const ids = new Set(this.participantIdsValue() ?? []);
    return this.selectedGroupUsers().filter(user => ids.has(user.id));
  });

  readonly pickupDateTime = computed(() => this.combineDateTime(this.pickupDateValue(), this.pickupTimeValue()));
  readonly dropOffDateTime = computed(() => this.combineDateTime(this.dropOffDateValue(), this.dropOffTimeValue()));
  readonly pickupDateMin = computed(() => this.resolvePickupDateMin());

  readonly summaryRequest = computed<CreateGarbageOrderRequest | null>(() => {
    const currentOption = this.serviceTypeValue();
    const selectedGroupId = this.selectedGroupId();
    const participants = this.participantIdsValue() ?? [];
    const pickupSchedule = this.pickupDateTime();
    const requiresDropOff = currentOption === PickupOption.Container;
    const dropOffSchedule = requiresDropOff ? this.dropOffDateTime() : null;
    if (currentOption === null || !selectedGroupId || !pickupSchedule || !participants.length) {
      return null;
    }

    if (requiresDropOff && !dropOffSchedule) {
      return null;
    }

    const containerSize = currentOption === PickupOption.Container ? this.containerSizeValue() ?? null : null;
    const isHighPriority = !!this.isHighPriorityValue();
    const collectingService = !!this.collectingServiceValue();

    return {
      pickupOption: currentOption,
      containerSize,
      dropOffDate: dropOffSchedule ? dropOffSchedule.isoString : null,
      pickupDate: pickupSchedule.isoString,
      isHighPriority,
      collectingService,
      userIds: participants
    };
  });

  readonly summaryReady = computed(() => this.summaryRequest() !== null);

  readonly costRequest = computed<CalculateGarbageOrderRequest | null>(() => {
    const pickupOption = this.serviceTypeValue();
    const pickupSchedule = this.pickupDateTime();

    if (pickupOption === null || pickupOption === PickupOption.SpecialOrder || !pickupSchedule) {
      return null;
    }

    const requiresContainer = pickupOption === PickupOption.Container;
    const containerSize = requiresContainer ? this.containerSizeValue() ?? null : null;
    const dropOffSchedule = requiresContainer ? this.dropOffDateTime() : null;

    if (requiresContainer && (containerSize === null || !dropOffSchedule)) {
      return null;
    }

    return {
      pickupOption,
      containerSize,
      dropOffDate: dropOffSchedule ? dropOffSchedule.isoString : null,
      pickupDate: pickupSchedule.isoString,
      isHighPriority: !!this.isHighPriorityValue(),
      collectingService: !!this.collectingServiceValue()
    };
  });

  readonly costLoading = signal(false);
  readonly estimatedCost = signal<number | null>(null);
  readonly estimatedTotalCost = signal<number | null>(null);
  readonly prepaidUtilizationFee = signal<number | null>(null);
  readonly costMessage = signal<string | null>(null);
  readonly costMessageType = signal<'info' | 'error'>('info');

  readonly selectedContainerSizeOption = computed(() => {
    const value = this.containerSizeValue();
    if (value === null) {
      return null;
    }
    return this.containerSizeOptions.find(option => option.value === value) ?? null;
  });

  readonly summaryPickupDate = computed(() => this.pickupDateTime()?.displayDate ?? null);
  readonly summaryDropOffDate = computed(() => this.dropOffDateTime()?.displayDate ?? null);

  readonly submitting = signal(false);
  readonly submitError = signal<string | null>(null);
  readonly submittedOrder = signal<GarbageOrderDto | null>(null);

  readonly pickupOptionEnum = PickupOption;

  readonly minPickupDate = this.todayDate();

  private readonly costCalculationEffect = effect((onCleanup) => {
    const step = this.currentStep();
    const groupId = this.selectedGroupId();
    const payload = this.costRequest();
    const pickupOption = this.serviceTypeValue();

    if (step !== this.summaryStepIndex) {
      this.resetCostState(true);
      return;
    }

    if (pickupOption === PickupOption.SpecialOrder) {
      this.resetCostState(false);
      this.setCostMessage('pickupOrder.summary.cost.specialOrder', 'info');
      return;
    }

    if (!groupId || !payload) {
      this.resetCostState(false);
      this.setCostMessage('pickupOrder.summary.cost.missing', 'info');
      return;
    }

    this.costLoading.set(true);
    this.estimatedCost.set(null);
    this.estimatedTotalCost.set(null);
    this.prepaidUtilizationFee.set(null);
    this.setCostMessage(null);

    const subscription = this.garbageOrderService
      .calculateOrderCost(groupId, payload)
      .pipe(finalize(() => this.costLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.errorMessage || !res.resultModel) {
            this.estimatedCost.set(null);
            this.estimatedTotalCost.set(null);
            this.prepaidUtilizationFee.set(null);
            this.setCostMessage(res.errorMessage ?? 'pickupOrder.summary.cost.error', 'error');
            return;
          }

          const result = res.resultModel;
          const totalCost = result.estimatedTotalCost ?? result.estimatedCost ?? null;

          this.estimatedCost.set(result.estimatedCost ?? null);
          this.estimatedTotalCost.set(totalCost);
          this.prepaidUtilizationFee.set(result.prepaidUtilizationFee ?? null);
          if (totalCost === null || totalCost === undefined) {
            this.setCostMessage('pickupOrder.summary.cost.unavailable', 'info');
          } else {
            this.setCostMessage(null);
          }
        },
        error: () => {
          this.estimatedCost.set(null);
          this.estimatedTotalCost.set(null);
          this.prepaidUtilizationFee.set(null);
          this.setCostMessage('pickupOrder.summary.cost.error', 'error');
        }
      });

    onCleanup(() => subscription.unsubscribe());
  }, { allowSignalWrites: true });

  ngOnInit(): void {
    this.configureDateLocale(this.translation.currentLang);
    this.translation
      .onLangChange
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(lang => this.configureDateLocale(lang));

    this.loadGroupsWithUsers();
    this.serviceTypeCtrl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(serviceType => this.handleServiceTypeChange(serviceType));
    this.containerSizeCtrl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(size => {
        if (size !== null) {
          this.lastSelectedContainerSize = size;
        }
      });
    this.handleServiceTypeChange(this.serviceTypeCtrl.value);
  }

  get serviceTypeCtrl() {
    return this.form.controls.serviceType;
  }

  get groupIdCtrl() {
    return this.form.controls.groupId;
  }

  get participantIdsCtrl() {
    return this.form.controls.participantIds;
  }

  get pickupDateCtrl() {
    return this.form.controls.pickupDate;
  }

  get pickupTimeCtrl() {
    return this.form.controls.pickupTime;
  }

  get dropOffDateCtrl() {
    return this.form.controls.dropOffDate;
  }

  get dropOffTimeCtrl() {
    return this.form.controls.dropOffTime;
  }

  get containerSizeCtrl() {
    return this.form.controls.containerSize;
  }

  get isHighPriorityCtrl() {
    return this.form.controls.isHighPriority;
  }

  get collectingServiceCtrl() {
    return this.form.controls.collectingService;
  }

  primaryActionDisabled(): boolean {
    const step = this.currentStep();
    if (step === 0) {
      return this.serviceTypeCtrl.invalid;
    }
    if (step === 1) {
      return this.groupIdCtrl.invalid || this.participantIdsCtrl.invalid;
    }
    if (step === 2) {
      const requiresDropOff = this.serviceTypeCtrl.value === PickupOption.Container;
      const scheduleInvalid = this.form.hasError('pickupBeforeDropOff');
      if (requiresDropOff) {
        return (
          this.dropOffDateCtrl.invalid ||
          this.dropOffTimeCtrl.invalid ||
          this.pickupDateCtrl.invalid ||
          this.pickupTimeCtrl.invalid ||
          scheduleInvalid
        );
      }
      return this.pickupDateCtrl.invalid || this.pickupTimeCtrl.invalid;
    }
    if (step === 3) {
      const requiresContainerSize = this.serviceTypeCtrl.value === PickupOption.Container;
      if (requiresContainerSize) {
        return this.containerSizeCtrl.invalid;
      }
    }
    if (step === this.summaryStepIndex) {
      return this.submitting() || !this.summaryReady();
    }
    return false;
  }

  onNext() {
    const step = this.currentStep();
    if (step === this.summaryStepIndex) {
      this.submitOrder();
      return;
    }

    if (step === 0) {
      this.serviceTypeCtrl.markAsTouched();
      this.serviceTypeCtrl.markAsDirty();
      if (this.serviceTypeCtrl.invalid) {
        return;
      }
    }

    if (step === 1) {
      this.groupIdCtrl.markAsTouched();
      this.groupIdCtrl.markAsDirty();
      this.participantIdsCtrl.markAsTouched();
      this.participantIdsCtrl.markAsDirty();
      if (this.groupIdCtrl.invalid || this.participantIdsCtrl.invalid) {
        return;
      }
    }

    if (step === 2) {
      this.pickupDateCtrl.markAsTouched();
      this.pickupDateCtrl.markAsDirty();
      this.pickupTimeCtrl.markAsTouched();
      this.pickupTimeCtrl.markAsDirty();
      const requiresDropOff = this.serviceTypeCtrl.value === PickupOption.Container;
      if (requiresDropOff) {
        this.dropOffDateCtrl.markAsTouched();
        this.dropOffDateCtrl.markAsDirty();
        this.dropOffTimeCtrl.markAsTouched();
        this.dropOffTimeCtrl.markAsDirty();
      }
      if (
        this.pickupDateCtrl.invalid ||
        this.pickupTimeCtrl.invalid ||
        (requiresDropOff && (this.dropOffDateCtrl.invalid || this.dropOffTimeCtrl.invalid))
      ) {
        return;
      }
      if (requiresDropOff && this.form.hasError('pickupBeforeDropOff')) {
        return;
      }
    }

    if (step === 3) {
      const requiresContainerSize = this.serviceTypeCtrl.value === PickupOption.Container;
      if (requiresContainerSize) {
        this.containerSizeCtrl.markAsTouched();
        this.containerSizeCtrl.markAsDirty();
        if (this.containerSizeCtrl.invalid) {
          return;
        }
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

  trackGroupById(_index: number, vm: GroupsWithUsersViewModel) {
    return vm.id;
  }

  trackUserById(_index: number, vm: GroupUserViewModel) {
    return vm.id;
  }

  onSelectGroup(groupId: string): void {
    this.groupIdCtrl.setValue(groupId);
    this.groupIdCtrl.markAsDirty();
    this.groupIdCtrl.markAsTouched();
    this.preselectParticipants(groupId);
  }

  onToggleParticipant(userId: string, checked: boolean): void {
    const selected = new Set(this.participantIdsCtrl.value ?? []);
    if (checked) {
      selected.add(userId);
    } else {
      selected.delete(userId);
    }
    this.participantIdsCtrl.setValue(Array.from(selected));
    this.participantIdsCtrl.markAsDirty();
    this.participantIdsCtrl.updateValueAndValidity();
  }

  private loadGroupsWithUsers(): void {
    this.loadingGroups.set(true);
    this.groupService
      .groupsWithUsers()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loadingGroups.set(false))
      )
      .subscribe((payload: Result<GarbageGroupWithUsers[]>) => {
        const groups = Array.isArray(payload?.resultModel) ? payload.resultModel : [];
        const mapped = groups.map<GroupsWithUsersViewModel>(group => ({
          id: group.groupId,
          name: group.groupName?.trim() || null,
          isPrivate: group.isPrivate,
          address: group.address,
          users: group.groupUsers.map<GroupUserViewModel>(user => ({
            id: user.id,
            username: user.username,
            disabled: user.isPending,
            role: user.garbageGroupRole
          }))
        }));

        const individual = mapped.find(item => item.isPrivate) ?? null;
        const nonPrivate = mapped.filter(item => !item.isPrivate);

        this.individualGroup.set(individual);
        this.groups.set(nonPrivate);

        const availableIds: string[] = [];
        if (individual) {
          availableIds.push(individual.id);
        }
        availableIds.push(...nonPrivate.map(item => item.id));

        let selectedGroupId = this.groupIdCtrl.value;
        const hasSelected = !!selectedGroupId && availableIds.includes(selectedGroupId);

        if (!hasSelected) {
          selectedGroupId = availableIds.length === 1 ? availableIds[0] : null;
          this.groupIdCtrl.setValue(selectedGroupId);
          this.groupIdCtrl.markAsPristine();
          this.groupIdCtrl.markAsUntouched();
        }

        this.preselectParticipants(selectedGroupId);
      });
  }

  groupRoleKey(role: GarbageGroupRole): string {
    switch (role) {
      case GarbageGroupRole.Owner:
        return 'groups.role.owner';
      case GarbageGroupRole.User:
      default:
        return 'groups.role.member';
    }
  }

  private preselectParticipants(groupId: string | null): void {
    let group: GroupsWithUsersViewModel | null = null;
    if (groupId) {
      group = this.groups().find(item => item.id === groupId) ?? null;
      if (!group) {
        const individual = this.individualGroup();
        if (individual && individual.id === groupId) {
          group = individual;
        }
      }
    }

    const selectableIds = group ? group.users.filter(user => !user.disabled).map(user => user.id) : [];
    this.participantIdsCtrl.setValue(selectableIds);
    this.participantIdsCtrl.markAsPristine();
    this.participantIdsCtrl.markAsUntouched();
    this.participantIdsCtrl.updateValueAndValidity();
  }

  private combineDateTime(date: string | Date | null, time: string | Date | null): PickupSchedule | null {
    const normalizedDate = this.normalizeDateInput(date);
    const normalizedTime = this.normalizeTimeInput(time);

    if (!normalizedDate || !normalizedTime) {
      return null;
    }

    const isoString = `${normalizedDate}T${normalizedTime}`;
    const displayDate = new Date(isoString);

    if (Number.isNaN(displayDate.getTime())) {
      return null;
    }

    return {
      isoString,
      displayDate
    };
  }

  private resetCostState(clearMessage: boolean): void {
    this.costLoading.set(false);
    this.estimatedCost.set(null);
    this.estimatedTotalCost.set(null);
    this.prepaidUtilizationFee.set(null);
    if (clearMessage) {
      this.setCostMessage(null);
    }
  }

  private setCostMessage(messageKey: string | null, type: 'info' | 'error' = 'info'): void {
    this.costMessage.set(messageKey);
    this.costMessageType.set(messageKey === null ? 'info' : type);
  }

  private submitOrder(): void {
    if (this.submitting()) {
      return;
    }

    const payload = this.summaryRequest();
    const groupId = this.selectedGroupId();

    if (!payload || !groupId) {
      this.submitError.set('pickupOrder.summary.incomplete');
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);
    this.submittedOrder.set(null);

    this.garbageOrderService
      .createOrder(groupId, payload)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.submitting.set(false))
      )
      .subscribe({
        next: (res) => {
          if (res.errorMessage || !res.resultModel) {
            this.submitError.set(res.errorMessage ?? 'pickupOrder.summary.submitError');
            this.submittedOrder.set(null);
            return;
          }

          this.submitError.set(null);
          this.submittedOrder.set(res.resultModel);
          this.showSuccessToast();
          this.scheduleNavigation(res.resultModel);
        },
        error: () => {
          this.submitError.set('pickupOrder.summary.submitError');
          this.submittedOrder.set(null);
        }
      });
  }

  private goToCreatedOrder(order?: GarbageOrderDto): void {
    const targetOrder = order ?? this.submittedOrder();
    const orderId = targetOrder?.id;
    if (!orderId) {
      return;
    }
    this.router.navigate(['/portal/my-pickups', orderId]);
  }

  private showSuccessToast(): void {
    const key = 'pickupOrder.summary.successToast';
    const translated = this.translation.translate(key);
    const fallback = this.translation.translate('success.update');
    const message = typeof translated === 'string' && translated !== key ? translated : fallback;
    this.toastr.success(message);
  }

  private scheduleNavigation(order: GarbageOrderDto): void {
    timer(this.navigationDelayMs)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.goToCreatedOrder(order));
  }

  private todayDate(): Date {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return today;
  }

  private normalizeDateInput(value: string | Date | null): string | null {
    if (!value) {
      return null;
    }

    if (typeof value === 'string') {
      return value;
    }

    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private normalizeTimeInput(value: string | Date | null): string | null {
    if (!value) {
      return null;
    }

    if (typeof value === 'string') {
      const trimmed = value.trim();
      if (!trimmed) {
        return null;
      }
      return trimmed.length === 5 ? `${trimmed}:00` : trimmed;
    }

    const hours = String(value.getHours()).padStart(2, '0');
    const minutes = String(value.getMinutes()).padStart(2, '0');
    const seconds = String(value.getSeconds()).padStart(2, '0');
    return `${hours}:${minutes}:${seconds}`;
  }

  private generateTimeOptions(): TimeOption[] {
    const options: TimeOption[] = [];
    for (let hour = 0; hour < 24; hour++) {
      for (let minute = 0; minute < 60; minute += 30) {
        const value = `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`;
        options.push({ value, label: value });
      }
    }
    return options;
  }

  private handleServiceTypeChange(serviceType: PickupOption | null): void {
    const isContainer = serviceType === PickupOption.Container;

    if (isContainer) {
      this.containerSizeCtrl.setValidators([Validators.required]);
      if (this.containerSizeCtrl.value === null) {
        this.containerSizeCtrl.setValue(this.lastSelectedContainerSize);
        this.containerSizeCtrl.markAsPristine();
        this.containerSizeCtrl.markAsUntouched();
      }
      this.dropOffDateCtrl.setValidators([Validators.required]);
      this.dropOffTimeCtrl.setValidators([Validators.required]);
    } else {
      this.containerSizeCtrl.clearValidators();
      if (this.containerSizeCtrl.value !== null) {
        this.containerSizeCtrl.setValue(null, { emitEvent: false });
      }
      this.containerSizeCtrl.markAsPristine();
      this.containerSizeCtrl.markAsUntouched();

      this.dropOffDateCtrl.clearValidators();
      this.dropOffTimeCtrl.clearValidators();
      if (this.dropOffDateCtrl.value !== null) {
        this.dropOffDateCtrl.setValue(null);
      }
      if (this.dropOffTimeCtrl.value !== null) {
        this.dropOffTimeCtrl.setValue(null);
      }
      this.dropOffDateCtrl.markAsPristine();
      this.dropOffDateCtrl.markAsUntouched();
      this.dropOffTimeCtrl.markAsPristine();
      this.dropOffTimeCtrl.markAsUntouched();
    }

    this.containerSizeCtrl.updateValueAndValidity({ emitEvent: false });
    this.dropOffDateCtrl.updateValueAndValidity({ emitEvent: false });
    this.dropOffTimeCtrl.updateValueAndValidity({ emitEvent: false });
    this.form.updateValueAndValidity({ emitEvent: false });
  }

  private validateForm(control: AbstractControl): ValidationErrors | null {
    const combinedErrors: ValidationErrors = {};

    const pickupErrors = this.validatePickupSchedule(control);
    if (pickupErrors) {
      Object.assign(combinedErrors, pickupErrors);
    }

    const containerErrors = this.validateContainerSchedule(control);
    if (containerErrors) {
      Object.assign(combinedErrors, containerErrors);
    }

    return Object.keys(combinedErrors).length > 0 ? combinedErrors : null;
  }

  private validatePickupSchedule(control: AbstractControl): ValidationErrors | null {
    const pickupDate = control.get('pickupDate')?.value ?? null;
    const pickupTime = control.get('pickupTime')?.value ?? null;

    const pickupSchedule = this.combineDateTime(pickupDate, pickupTime);
    if (!pickupSchedule) {
      return null;
    }

    const now = Date.now();
    const pickupTimestamp = pickupSchedule.displayDate.getTime();

    if (Number.isNaN(pickupTimestamp)) {
      return null;
    }

    if (pickupTimestamp < now) {
      return { pickupInPast: true };
    }

    return null;
  }

  private validateContainerSchedule(control: AbstractControl): ValidationErrors | null {
    const serviceType = control.get('serviceType')?.value as PickupOption | null;
    if (serviceType !== PickupOption.Container) {
      return null;
    }

    const dropOffDate = control.get('dropOffDate')?.value ?? null;
    const dropOffTime = control.get('dropOffTime')?.value ?? null;
    const pickupDate = control.get('pickupDate')?.value ?? null;
    const pickupTime = control.get('pickupTime')?.value ?? null;

    const dropOffSchedule = this.combineDateTime(dropOffDate, dropOffTime);
    const pickupSchedule = this.combineDateTime(pickupDate, pickupTime);

    if (!dropOffSchedule || !pickupSchedule) {
      return null;
    }

    const dropOffTimeValue = dropOffSchedule.displayDate.getTime();
    const pickupTimeValue = pickupSchedule.displayDate.getTime();

    if (pickupTimeValue < dropOffTimeValue) {
      return { pickupBeforeDropOff: true };
    }

    return null;
  }

  private resolvePickupDateMin(): Date {
    const requiresDropOff = this.serviceTypeValue() === PickupOption.Container;
    if (!requiresDropOff) {
      return this.cloneDateOnly(this.minPickupDate);
    }

    const dropOffRaw = this.dropOffDateValue();
    if (!dropOffRaw) {
      return this.cloneDateOnly(this.minPickupDate);
    }

    if (dropOffRaw instanceof Date) {
      return this.cloneDateOnly(dropOffRaw);
    }

    const parsed = new Date(dropOffRaw);
    if (Number.isNaN(parsed.getTime())) {
      return this.cloneDateOnly(this.minPickupDate);
    }

    return this.cloneDateOnly(parsed);
  }

  private cloneDateOnly(source: Date): Date {
    const copy = new Date(source);
    copy.setHours(0, 0, 0, 0);
    return copy;
  }

  private configureDateLocale(language: string): void {
    const normalized = (language || '').toLowerCase();
    const locale = normalized === 'pl' ? 'pl-PL' : 'en-US';
    this.dateAdapter.setLocale(locale);
  }
}
