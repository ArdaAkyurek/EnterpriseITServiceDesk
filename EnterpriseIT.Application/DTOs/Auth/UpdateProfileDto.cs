using System.ComponentModel.DataAnnotations;

namespace EnterpriseIT.Application.DTOs.Auth;

public class UpdateProfileDto
{
    [StringLength(20)]
    public string? Phone { get; set; }
}
