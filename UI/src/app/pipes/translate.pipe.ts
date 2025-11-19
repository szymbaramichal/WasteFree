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

  transform(key: string, params?: Record<string, unknown>): string {
    const base = this.t.translate(key);
    if (!params || typeof base !== 'string') {
      return base;
    }

    return base.replace(/\{\{\s*(\w+)\s*\}\}|\{\s*(\w+)\s*\}/g, (_match, token1, token2) => {
      const token = token1 || token2;
      const value = params[token];
      return value === undefined || value === null ? '' : String(value);
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
