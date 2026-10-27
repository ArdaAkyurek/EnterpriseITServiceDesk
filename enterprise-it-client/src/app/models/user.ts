export interface ManagedUser {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  isActive: boolean;
  departmentId: number;
  roleId: number;
}

export interface CreateUser extends Omit<ManagedUser, 'userId'> {
  password: string;
}

export interface UpdateUser extends Omit<ManagedUser, 'userId'> {
  password?: string;
}

export interface UserRoleOption { roleId: number; roleName: string; description?: string | null; }
export interface UserDepartmentOption { departmentId: number; departmentName: string; }
