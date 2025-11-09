import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '@app/pipes/translate.pipe';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { GarbageGroupService } from '@app/services/garbage-group.service';
import { GarbageGroupWithUsers, GarbageGroupRole } from '@app/_models/garbageGroups';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { Result } from '@app/_models/result';

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

@Component({
  selector: 'app-pickup-order',
  standalone: true,
  imports: [CommonModule, TranslatePipe, ReactiveFormsModule],
  templateUrl: './pickup-order.component.html',
  styleUrls: ['./pickup-order.component.css']
})
export class PickupOrderComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly groupService = inject(GarbageGroupService);
  private readonly destroyRef = inject(DestroyRef);

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
    serviceType: this.fb.control<number | null>(null, { validators: Validators.required }),
    groupId: this.fb.control<string | null>(null, { validators: Validators.required }),
    participantIds: this.fb.control<string[]>([], { validators: Validators.required }),
    pickupDate: this.fb.control<string | null>(null, { validators: Validators.required }),
    pickupTime: this.fb.control<string | null>(null, { validators: Validators.required })
  });

  private readonly groupIdValue = toSignal(this.groupIdCtrl.valueChanges, {
    initialValue: this.groupIdCtrl.value
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
    const current = this.serviceTypeCtrl.value;
    return this.serviceOptions.find(option => option.value === current) ?? null;
  });

  readonly minPickupDate = this.todayIsoDate();

  ngOnInit(): void {
    this.loadGroupsWithUsers();
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

  primaryActionDisabled(): boolean {
    const step = this.currentStep();
    if (step === 0) {
      return this.serviceTypeCtrl.invalid;
    }
    if (step === 1) {
      return this.groupIdCtrl.invalid || this.participantIdsCtrl.invalid;
    }
    if (step === 2) {
      return this.pickupDateCtrl.invalid || this.pickupTimeCtrl.invalid;
    }
    return step >= this.steps.length - 1;
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
      if (this.pickupDateCtrl.invalid || this.pickupTimeCtrl.invalid) {
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

  private todayIsoDate(): string {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
