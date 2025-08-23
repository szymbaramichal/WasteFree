import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthComponent } from '../auth/auth.component';

@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [AuthComponent],
  templateUrl: './portal.component.html',
  styleUrl: './portal.component.css'
})
export class PortalComponent {
  private bodyClass = 'portal-bg';

  ngOnInit(): void {
    document.body.classList.add(this.bodyClass);
  }

  ngOnDestroy(): void {
    document.body.classList.remove(this.bodyClass);
  }

}
