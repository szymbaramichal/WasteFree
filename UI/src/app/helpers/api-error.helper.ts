export function extractApiErrorPayload(p: any): string {
  console.log('[api-error] input:', p);

  if (!p) return '';
  if (typeof p !== 'object') {
    if (typeof p === 'string') return p.trim();
    return '';
  }

  if (p && p.error && typeof p.error === 'object') {
    p = p.error;
  }

  const bag: any = (p.errors && typeof p.errors === 'object' && !Array.isArray(p.errors)) ? p.errors : p;
  console.log('[api-error] bag:', bag);
  if (!bag || typeof bag !== 'object' || Array.isArray(bag)) return '';

  const messages: string[] = [];
  for (const [key, raw] of Object.entries(bag)) {
    if (Array.isArray(raw)) {
      for (const item of raw) {
        if (typeof item === 'string') {
          const t = item.trim();
          if (t) messages.push(t);
        }
      }
    } else if (typeof raw === 'string') {
      const t = raw.trim();
      if (t) messages.push(t);
    } else if (raw && typeof raw === 'object' && Array.isArray((raw as any).messages)) {
      for (const m of (raw as any).messages) {
        if (typeof m === 'string') {
          const t = m.trim();
          if (t) messages.push(t);
        }
      }
    }
  }

  const unique = messages.filter((m, i) => messages.indexOf(m) === i);
  const joined = unique.join('\n');
  console.log('[api-error] extracted messages:', unique);
  console.log('[api-error] final string:', joined);
  return joined;
}
