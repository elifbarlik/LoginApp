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
import { GoogleIdentityService } from '../../../core/services/google-identity.service';

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

  constructor(private auth: AuthService, private router: Router, private google: GoogleIdentityService) {}

  onSubmit() {
    if (!this.email || !this.password) return;
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: (e) => this.error = (e?.error?.message || 'Giriş başarısız')
    });
  }

  loginWithGoogle() {
    this.error = '';
    this.google.signIn().then(idToken => {
      this.auth.loginWithGoogle(idToken).subscribe({
        next: () => this.router.navigateByUrl('/'),
        error: (e) => this.error = (e?.error?.message || 'Google ile giriş başarısız')
      });
    }).catch(err => this.error = 'Google ile giriş iptal edildi');
  }
}
