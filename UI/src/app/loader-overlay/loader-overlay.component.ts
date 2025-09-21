import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoaderService } from '../services/loader.service';
import { AsyncPipe } from '@angular/common';
import { TranslatePipe } from '../pipes/translate.pipe';

@Component({
  selector: 'app-loader-overlay',
  standalone: true,
  imports: [CommonModule, AsyncPipe, TranslatePipe],
  templateUrl: './loader-overlay.component.html',
  styleUrls: ['./loader-overlay.component.css']
})
export class LoaderOverlayComponent {
  constructor(public loader: LoaderService) {}
}
