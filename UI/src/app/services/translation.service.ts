import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TranslationService {
  private lang = 'pl';
  private cache: Record<string, any> = {};
  private lang$ = new BehaviorSubject<string>(this.lang);

  constructor(private http: HttpClient) {
    const stored = localStorage.getItem('lang');
    if (stored) this.lang = stored;
  // don't eagerly block app here; app initializer will ensure loading when configured
  this.loadLang(this.lang);
  }

  private normalizeLang(lang: string | undefined): string {
    if (!lang) return 'pl';
    const m = /^([a-z]{2})/i.exec(lang);
    return m ? m[1].toLowerCase() : lang.toLowerCase();
  }

  get onLangChange() {
    return this.lang$.asObservable();
  }

  get currentLang() {
    return this.lang;
  }

  private loadLang(lang: string) {
    this.loadLangInternal(lang, false);
  }

  // Promise-based loader usable by APP_INITIALIZER to block bootstrap until translations are ready
  async loadLangPromise(lang?: string): Promise<void> {
    const targetRaw = lang || this.lang;
    const target = this.normalizeLang(targetRaw);
    if (this.cache[target]) return;
    try {
      const obs = this.http.get(`/assets/i18n/${target}.json`);
      const res = await firstValueFrom(obs);
      this.cache[target] = res || {};
      this.lang = target;
      localStorage.setItem('lang', this.lang);
      this.lang$.next(this.lang);
    } catch (e) {
      this.cache[target] = {};
      // still set lang and notify so pipe can update
      this.lang = target;
      this.lang$.next(this.lang);
    }
  }

  translate(key: string): string {
    if (!key) return '';
    const dict = this.cache[this.lang] || {};
    // First, try flat key (e.g. { "home.title": "..." })
    if (typeof dict[key] === 'string') return dict[key];

    // Fallback: try nested lookup for objects like { home: { title: "..." } }
    const parts = key.split('.');
    let cur: any = dict;
    for (const p of parts) {
      if (cur && typeof cur === 'object' && p in cur) cur = cur[p];
      else return key; // fallback to key if not found
    }
    return typeof cur === 'string' ? cur : key;
  }

  /**
   * Internal loader allowing forced reload when a previous attempt failed (e.g. cached as empty).
   */
  private loadLangInternal(lang: string, force: boolean) {
    const short = this.normalizeLang(lang);
    const existing = this.cache[short];
    // Only skip if we already have a non-empty dictionary and not forcing
    if (!force && existing && Object.keys(existing).length > 0) return;

    this.http.get(`/assets/i18n/${short}.json`).subscribe(
      (res) => {
        this.cache[short] = (res as any) || {};
        this.lang$.next(this.lang); // notify subscribers when loaded
      },
      () => {
        // On error do NOT persist an empty object permanently; allow future retries
        delete this.cache[short];
      }
    );
  }

  /**
   * Enhanced setLanguage: if selecting same language but cache is empty, retry loading.
   */
  setLanguage(lang: string) {
    if (!lang) return;
    const short = this.normalizeLang(lang);
    // If selecting the same language, but we have no loaded keys, force reload
    if (this.lang === short) {
      const existing = this.cache[short];
      if (!existing || Object.keys(existing).length === 0) {
        this.loadLangInternal(short, true);
      }
      return;
    }
    this.lang = short;
    localStorage.setItem('lang', short);
    this.loadLangInternal(short, false);
    this.lang$.next(short);
  }
}
