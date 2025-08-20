import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  serverErrors: Record<string, string[]> = {};

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit() {
    if (!this.email || !this.password) return;
    this.error = '';
    this.serverErrors = {};
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: (e) => {
        const body = e?.error;
        const msg = body?.message as string | undefined;
        this.error = msg || 'Giriş başarısız';

        const errors = body?.errors as Array<{ field: string; messages: string[] }> | undefined;
        if (Array.isArray(errors)) {
          const map: Record<string, string[]> = {};
          for (const err of errors) {
            const rawKey = (err.field || '').toString();
            const keyParts = rawKey.split('.');
            const key = (keyParts[keyParts.length - 1] || '').toLowerCase();
            if (!key) continue;
            map[key] = (map[key] || []).concat(err.messages || []);
          }
          this.serverErrors = map;
        }
      }
    });
  }

  
}
