import { CommonModule } from '@angular/common';
import { Component, computed, effect, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@app/pipes/translate.pipe';

interface PortalPickupItem {
  id: string;
  orderNumber: string;
  groupId: string;
  groupName: string;
  status: 'pending' | 'scheduled' | 'completed';
  serviceCost: number;
  collectDate: string;
  pickupOption: 'regular' | 'container' | 'special';
}

type StatusFilter = PortalPickupItem['status'] | 'all';
type GroupFilter = PortalPickupItem['groupId'] | 'all';

@Component({
  selector: 'app-my-pickups',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslatePipe],
  templateUrl: './my-pickups.component.html',
  styleUrls: ['./my-pickups.component.css']
})
export class MyPickupsComponent {
  // TODO: replace with API data once pickups endpoint is connected
  private placeholderData: PortalPickupItem[] = [
    {
      id: '1',
      orderNumber: 'WF-2025-001',
      groupId: 'group-1',
      groupName: 'Recycling Heroes',
      status: 'scheduled',
      serviceCost: 189.9,
      collectDate: '2025-11-12T09:00:00Z',
      pickupOption: 'regular'
    },
    {
      id: '2',
      orderNumber: 'WF-2025-002',
      groupId: 'group-2',
      groupName: 'Green Neighbourhood',
      status: 'pending',
      serviceCost: 89.5,
      collectDate: '2025-11-18T14:30:00Z',
      pickupOption: 'special'
    },
    {
      id: '3',
      orderNumber: 'WF-2025-003',
      groupId: 'group-3',
      groupName: 'Waste Less Initiative',
      status: 'completed',
      serviceCost: 249.0,
      collectDate: '2025-10-24T07:45:00Z',
      pickupOption: 'container'
    }
  ];

  readonly loading = signal(false);
  readonly pickups = signal<PortalPickupItem[]>(this.placeholderData);
  readonly itemsPerPage = 10;
  readonly statusOptions: PortalPickupItem['status'][] = ['pending', 'scheduled', 'completed'];
  readonly statusFilter = signal<StatusFilter>('all');
  readonly groupFilter = signal<GroupFilter>('all');
  readonly currentPage = signal(1);
  readonly filteredPickups = computed(() => {
    const statusFilter = this.statusFilter();
    const groupFilter = this.groupFilter();
    return this.pickups().filter((pickup) => {
      const matchesStatus = statusFilter === 'all' || pickup.status === statusFilter;
      const matchesGroup = groupFilter === 'all' || pickup.groupId === groupFilter;
      return matchesStatus && matchesGroup;
    });
  });
  readonly groupOptions = computed(() => {
    const groups = new Map<string, string>();
    for (const pickup of this.pickups()) {
      if (!groups.has(pickup.groupId)) {
        groups.set(pickup.groupId, pickup.groupName);
      }
    }
    return Array.from(groups.entries()).map(([id, name]) => ({ id, name }));
  });
  readonly totalPages = computed(() => Math.ceil(this.filteredPickups().length / this.itemsPerPage));
  readonly paginatedPickups = computed(() => {
    const start = (this.currentPage() - 1) * this.itemsPerPage;
    return this.filteredPickups().slice(start, start + this.itemsPerPage);
  });
  readonly pages = computed(() => {
    const total = this.totalPages();
    return Array.from({ length: total }, (_value, index) => index + 1);
  });
  readonly pageStartIndex = computed(() => {
    if (!this.filteredPickups().length) {
      return 0;
    }
    return (this.currentPage() - 1) * this.itemsPerPage;
  });
  readonly pageEndIndex = computed(() => {
    if (!this.filteredPickups().length) {
      return 0;
    }
    return Math.min(this.pageStartIndex() + this.itemsPerPage, this.filteredPickups().length);
  });

  constructor() {
    effect(() => {
      const total = this.totalPages();
      const current = this.currentPage();
      if (total === 0) {
        if (current !== 1) {
          this.currentPage.set(1);
        }
        return;
      }

      if (current > total) {
        this.currentPage.set(total);
      }
    });
  }

  trackByOrder(_index: number, pickup: PortalPickupItem): string {
    return pickup.orderNumber;
  }

  statusClass(status: PortalPickupItem['status']): string {
    switch (status) {
      case 'completed':
        return 'status-chip status-completed';
      case 'scheduled':
        return 'status-chip status-scheduled';
      default:
        return 'status-chip status-pending';
    }
  }

  pickupOptionKey(option: PortalPickupItem['pickupOption']): string {
    return `myPickups.option.${option}`;
  }

  statusTranslationKey(status: PortalPickupItem['status']): string {
    return `myPickups.status.${status}`;
  }

  setStatusFilter(value: StatusFilter): void {
    this.statusFilter.set(value);
    this.currentPage.set(1);
  }

  setGroupFilter(value: GroupFilter): void {
    this.groupFilter.set(value);
    this.currentPage.set(1);
  }

  goToPage(page: number): void {
    const total = this.totalPages();
    if (total === 0) {
      this.currentPage.set(1);
      return;
    }

    const clamped = Math.min(Math.max(page, 1), total);
    this.currentPage.set(clamped);
  }

  previousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.update((current) => current - 1);
    }
  }

  nextPage(): void {
    const total = this.totalPages();
    if (total > 0 && this.currentPage() < total) {
      this.currentPage.update((current) => current + 1);
    }
  }
}
