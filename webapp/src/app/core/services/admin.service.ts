import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of, switchMap, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';

export type AdminUserDto = {
  id: string;
  email: string;
  role: 'User' | 'Admin';
  username?: string | null;
  phone?: string | null;
  address?: string | null;
  createdAtUtc?: string;
  lastLoginAtUtc?: string | null;
};

export type AdminCreateUserRequest = {
  email: string;
  password: string;
  role: 'User' | 'Admin';
};

export type AdminUpdateUserRequest = {
  email: string;
  role?: 'User' | 'Admin';
};

export type AdminStatsDto = {
  totalUsers: number;
  byRole: { [key: string]: number };
  latestUsers: Array<Pick<AdminUserDto, 'id' | 'email' | 'role' | 'createdAtUtc'>>;
};

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getUsers(role?: 'User' | 'Admin'): Observable<AdminUserDto[]> {
    const buildUrl = (useQuery: boolean) =>
      role
        ? (useQuery
            ? `${this.baseUrl}/admin/users?role=${encodeURIComponent(role)}`
            : `${this.baseUrl}/admin/users/${role}`)
        : `${this.baseUrl}/admin/users`;

    const extract = (obj: any): AdminUserDto[] => {
      if (!obj) return [];
      if (Array.isArray(obj)) return obj as AdminUserDto[];
      if (Array.isArray(obj.items)) return obj.items as AdminUserDto[];
      if (Array.isArray(obj.data)) return obj.data as AdminUserDto[];
      if (Array.isArray(obj.users)) return obj.users as AdminUserDto[];
      for (const key of ['result', 'payload', 'response', 'content']) {
        const val = obj[key];
        if (Array.isArray(val)) return val as AdminUserDto[];
        if (val && typeof val === 'object') {
          const nested = extract(val);
          if (nested.length) return nested;
        }
      }
      return [] as AdminUserDto[];
    };

    // Try path variant first; if it errors or returns empty while role is specified, fallback to query variant
    return this.http.get<any>(buildUrl(false)).pipe(
      map((res: any) => extract(res)),
      catchError(() => of([] as AdminUserDto[])),
      switchMap((arr: AdminUserDto[]) => {
        if (!role) return of(arr);
        if (Array.isArray(arr) && arr.length > 0) return of(arr);
        return this.http.get<any>(buildUrl(true)).pipe(
          map((res: any) => extract(res)),
          catchError(() => of([] as AdminUserDto[]))
        );
      })
    );
  }

  getUser(id: string): Observable<AdminUserDto> {
    return this.http.get<AdminUserDto>(`${this.baseUrl}/admin/users/${id}`);
    
  }

  createUser(body: AdminCreateUserRequest): Observable<AdminUserDto> {
    return this.http.post<AdminUserDto>(`${this.baseUrl}/admin/users`, body);
  }

  updateUser(id: string, body: AdminUpdateUserRequest): Observable<AdminUserDto> {
    return this.http.put<AdminUserDto>(`${this.baseUrl}/admin/users/${id}`, body);
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/admin/users/${id}`);
  }

  getStats(): Observable<AdminStatsDto> {
    return this.http.get<AdminStatsDto>(`${this.baseUrl}/admin/stats`);
  }
}



