using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Application.DTOs.User
{
    public class UserDto
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Keys
        public int DepartmentId { get; set; }

        public int RoleId { get; set; }
    }
}
