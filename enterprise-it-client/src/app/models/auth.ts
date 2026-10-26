export type UserRole = 'Admin' | 'Technician' | 'Employee';

export interface AuthUser {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  user: AuthUser;
}
