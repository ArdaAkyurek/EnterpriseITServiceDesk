using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseIT.Domain.Entities;

namespace EnterpriseIT.Application.Interfaces.Repositories;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<DepartmentWriteResult> CreateDepartmentAsync(Department department);
    Task<DepartmentWriteResult> UpdateDepartmentAsync(Department department);
    Task<DepartmentWriteResult> DeleteDepartmentAsync(int id);
}

public enum DepartmentWriteResult { Success, NotFound, DuplicateName, HasUsers }
