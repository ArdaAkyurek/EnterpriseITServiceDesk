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

export interface UserProfile extends AuthUser {
  department: string;
  phone: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface ChangePassword {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
