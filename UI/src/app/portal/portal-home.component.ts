import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-portal-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h3 class="h5 mb-3">Dashboard</h3>
      <p class="text-muted">Welcome to your portal. Choose an option from the left menu.</p>
    </div>
  `
})
export class PortalHomeComponent {}
