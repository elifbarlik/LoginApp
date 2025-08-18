import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export type UserProfileDto = {
  id: string;
  email: string;
  username?: string | null;
  role: string;
  phone?: string | null;
  address?: string | null;
  createdAtUtc: string;
  lastLoginAtUtc?: string | null;
};

export type UpdateProfileRequest = {
  email?: string;
  username?: string;
  phone?: string;
  address?: string;
};

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(`${this.baseUrl}/user/profile`);
  }

  updateProfile(body: UpdateProfileRequest): Observable<UserProfileDto> {
    return this.http.put<UserProfileDto>(`${this.baseUrl}/user/profile`, body);
  }
}


