import { Component, OnInit, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule, NgIf, NgForOf, NgClass, SlicePipe } from '@angular/common';
import { GarbageGroupInfo } from '@app/_models/garbageGroups';
import { GarbageGroupService } from '@app/services/garbage-group.service';
import { HttpClientModule } from '@angular/common/http';
import { finalize } from 'rxjs';
import { TranslatePipe } from '@app/pipes/translate.pipe';

@Component({
  selector: 'app-groups',
  standalone: true,
  imports: [RouterModule, CommonModule, TranslatePipe, NgIf, NgForOf, NgClass, SlicePipe, HttpClientModule],
  templateUrl: './groups.component.html',
  styleUrls: ['./groups.component.css']
})
export class GroupsComponent implements OnInit {
  groups: GarbageGroupInfo[] = [];
  loading = false;
  loadError: string | null = null;
  groupService = inject(GarbageGroupService);

  fetchGroups(): void {
    this.loading = true;
    this.loadError = null;
    this.groupService.list()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: res => {
          this.groups = res.resultModel || [];
        },
        error: err => {
          this.loadError = err?.error?.errorMessage;
        }
      });
  }

  ngOnInit(): void {
    this.fetchGroups();
  }

  avatarColor(name: string): string {
    if(!name) return '#6c757d';
    const hash = Array.from(name).reduce((acc, ch) => acc + ch.charCodeAt(0), 0);
    const colors = ['#2bb673', '#1f8b56', '#198754', '#0d6efd', '#20c997', '#6f42c1', '#fd7e14'];
    return colors[hash % colors.length];
  }

}
