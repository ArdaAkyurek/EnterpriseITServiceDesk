using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace EnterpriseIT.Application.DTOs.User
{
    public class UpdateUserDto
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(128, MinimumLength = 12)]
        public string? Password { get; set; }

        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Keys
        public int DepartmentId { get; set; }

        public int RoleId { get; set; }
    }
}
