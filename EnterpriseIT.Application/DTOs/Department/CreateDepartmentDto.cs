using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.ComponentModel.DataAnnotations;

namespace EnterpriseIT.Application.DTOs.Department;

public class CreateDepartmentDto
{
    [Required, StringLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [StringLength(250)]
    public string? DepartmentDescription { get; set; }
}
