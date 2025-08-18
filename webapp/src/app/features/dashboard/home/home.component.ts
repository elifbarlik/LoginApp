import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, UserProfileDto } from '../../../core/services/user.service';
import { AdminService, AdminStatsDto, AdminUserDto } from '../../../core/services/admin.service';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, MatToolbarModule, MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatDividerModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  profile?: UserProfileDto;
  editingEmail = '';
  editingUsername = '';
  editingPhone = '';
  editingAddress = '';
  loading = true;
  error = '';
  success = '';
  // Admin state
  isAdmin = false;
  users: AdminUserDto[] = [];
  usersLoading = false;
  stats?: AdminStatsDto;
  statsLoading = false;
  filterRole: 'User' | 'Admin' | '' = '';
  newUser = { email: '', password: '', role: 'User' as 'User' | 'Admin' };
  selectedUserId: string | null = null;
  updateUserEmail = '';
  updateUserRole: 'User' | 'Admin' = 'User';

  constructor(private auth: AuthService, private userService: UserService, private router: Router, private admin: AdminService) {}

  ngOnInit() {
    this.userService.getProfile().subscribe({
      next: (p) => {
        this.profile = p;
        this.editingEmail = p.email;
        this.editingUsername = p.username ?? '';
        this.editingPhone = p.phone ?? '';
        this.editingAddress = p.address ?? '';
        this.loading = false;
        this.isAdmin = (this.auth.getRole() === 'Admin');
        if (this.isAdmin) {
          this.loadAdminData();
        }
      },
      error: (e) => { this.error = (e?.error?.message || 'Profil yüklenemedi'); this.loading = false; }
    });
  }

  saveProfile() {
    const body: any = {};
    if (this.editingEmail) body.email = this.editingEmail;
    if (this.editingUsername) body.username = this.editingUsername;
    if (this.editingPhone) body.phone = this.editingPhone;
    if (this.editingAddress) body.address = this.editingAddress;
    this.userService.updateProfile(body).subscribe({
      next: (p) => { this.profile = p; this.success = 'Profil güncellendi'; this.error = ''; },
      error: (e) => { this.success = ''; this.error = (e?.error?.message || 'Güncelleme başarısız'); }
    });
  }

  // Admin actions
  private loadAdminData() {
    this.fetchUsers();
    this.fetchStats();
  }

  fetchUsers() {
    this.usersLoading = true;
    const role = this.filterRole || undefined;
    this.admin.getUsers(role as any).subscribe({
      next: (u) => {
        this.users = u ?? [];
        this.usersLoading = false;
      },
      error: (e) => {
        this.error = (e?.error?.message || 'Kullanıcılar yüklenemedi');
        this.users = [];
        this.usersLoading = false;
      }
    });
  }

  fetchStats() {
    this.statsLoading = true;
    this.admin.getStats().subscribe({
      next: (s) => { this.stats = s; this.statsLoading = false; },
      error: (e) => { this.error = (e?.error?.message || 'İstatistikler yüklenemedi'); this.statsLoading = false; }
    });
  }

  createUser() {
    if (!this.newUser.email || !this.newUser.password) return;
    this.admin.createUser(this.newUser).subscribe({
      next: () => { this.success = 'Kullanıcı oluşturuldu'; this.error = ''; this.newUser = { email: '', password: '', role: 'User' }; this.fetchUsers(); },
      error: (e) => { this.success = ''; this.error = (e?.error?.message || 'Kullanıcı oluşturma başarısız'); }
    });
  }

  selectUser(u: AdminUserDto) {
    this.selectedUserId = u.id;
    this.updateUserEmail = u.email;
    this.updateUserRole = u.role;
  }

  updateSelectedUser() {
    if (!this.selectedUserId || !this.updateUserEmail) return;
    this.admin.updateUser(this.selectedUserId, { email: this.updateUserEmail, role: this.updateUserRole }).subscribe({
      next: (u) => { this.success = 'Kullanıcı güncellendi'; this.error = ''; this.fetchUsers(); },
      error: (e) => { this.success = ''; this.error = (e?.error?.message || 'Kullanıcı güncelleme başarısız'); }
    });
  }

  deleteUser(id: string) {
    this.admin.deleteUser(id).subscribe({
      next: () => { this.success = 'Kullanıcı silindi'; this.error = ''; this.fetchUsers(); },
      error: (e) => { this.success = ''; this.error = (e?.error?.message || 'Kullanıcı silme başarısız'); }
    });
  }

  logout() {
    this.auth.logout().subscribe({
      next: () => this.router.navigateByUrl('/login'),
      error: () => this.router.navigateByUrl('/login')
    });
  }
}
