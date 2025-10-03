export function extractApiErrorPayload(p: any): string[] {
    const bag: any = (p && typeof p === 'object' && p.errors && typeof p.errors === 'object') ? p.errors : p;

    try {
      const values = Object.values(bag as Record<string, unknown>);
      const messages = values
        .flatMap((v: any) => Array.isArray(v) ? v : [v])
        .filter((m: any) => typeof m === 'string' && m.trim())
        .map((m: string) => m.trim());
      return messages;
    } catch {
      return [];
    }
}
