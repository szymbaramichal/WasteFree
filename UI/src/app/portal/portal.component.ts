import { Component } from '@angular/core';
import { AuthComponent } from '../auth/auth.component';

@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [AuthComponent],
  templateUrl: './portal.component.html',
  styleUrl: './portal.component.css'
})
export class PortalComponent {

}
