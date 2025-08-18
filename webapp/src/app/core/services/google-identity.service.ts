import { Injectable, Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';

declare global {
  interface Window { google?: any }
}

@Injectable({ providedIn: 'root' })
export class GoogleIdentityService {
  private initialized = false;
  private clientId: string | null = null;

  constructor(@Inject(DOCUMENT) private document: Document) {}

  private ensureScript(clientId: string): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      if (this.initialized && this.clientId === clientId) return resolve();

      const existing = this.document.getElementById('google-identity');
      if (!existing) {
        const script = this.document.createElement('script');
        script.id = 'google-identity';
        script.src = 'https://accounts.google.com/gsi/client';
        script.async = true;
        script.defer = true;
        script.onload = () => resolve();
        script.onerror = () => reject('Failed to load Google script');
        this.document.head.appendChild(script);
      } else {
        resolve();
      }
    }).then(() => {
      this.initialized = true;
      this.clientId = clientId;
    });
  }

  async signIn(): Promise<string> {
    const clientId = (window as any).env?.googleClientId || this.readMeta('google-client-id');
    if (!clientId) throw new Error('Google Client ID missing');
    await this.ensureScript(clientId);

    return new Promise<string>((resolve, reject) => {
      if (!window.google?.accounts?.id) return reject('Google Identity not available');

      let handled = false;
      const handleCredential = (resp: any) => {
        if (handled) return; handled = true;
        if (resp?.credential) resolve(resp.credential);
        else reject('No credential');
      };

      window.google.accounts.id.initialize({ client_id: clientId, callback: handleCredential });
      window.google.accounts.id.prompt((notification: any) => {
        if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
          // fallback to one-tap button
          const div = this.document.createElement('div');
          this.document.body.appendChild(div);
          window.google.accounts.id.renderButton(div, { type: 'standard', size: 'large', text: 'continue_with', locale: 'en' });
        }
      });
    });
  }

  private readMeta(name: string): string | null {
    const meta = this.document.querySelector(`meta[name="${name}"]`) as HTMLMetaElement | null;
    return meta?.content || null;
  }
}


