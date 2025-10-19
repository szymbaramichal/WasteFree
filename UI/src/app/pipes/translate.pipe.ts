import { Pipe, PipeTransform, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { TranslationService } from '../services/translation.service';
import { Subscription } from 'rxjs';

@Pipe({
  name: 'translate',
  standalone: true,
  pure: false
})
export class TranslatePipe implements PipeTransform, OnDestroy {
  private sub: Subscription | null = null;

  constructor(private t: TranslationService, private cdr: ChangeDetectorRef) {
    this.sub = this.t.onLangChange.subscribe(() => {
      try { 
        this.cdr.markForCheck(); 
      } 
      catch 
      { /* noop in some envs */ }
    });
  }

  transform(key: string): string {
    return this.t.translate(key);
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
