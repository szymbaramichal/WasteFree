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
    // subscribe to language changes so pipe updates templates
    this.sub = this.t.onLangChange.subscribe(() => {
      // mark for check so impure pipe results are refreshed
      try { this.cdr.markForCheck(); } catch { /* noop in some envs */ }
    });
  }

  transform(key: string): string {
    return this.t.translate(key);
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
