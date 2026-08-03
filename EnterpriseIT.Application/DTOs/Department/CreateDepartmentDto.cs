using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EnterpriseIT.Application.DTOs.Department;

public class CreateDepartmentDto
{
    public string DepartmentName { get; set; } = string.Empty;

    public string? DepartmentDescription { get; set; }
}