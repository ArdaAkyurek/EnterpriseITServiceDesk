using EnterpriseIT.Domain.Common;

namespace EnterpriseIT.Domain.Entities
{
    public class Department : BaseEntity
    {
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public string? DepartmentDescription { get; set; }

        // Navigation Property
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}