export interface Department {
  departmentId: number;
  departmentName: string;
  departmentDescription: string | null;
}

export type SaveDepartment = Omit<Department, 'departmentId'>;
