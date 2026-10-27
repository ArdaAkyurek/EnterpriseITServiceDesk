using System.ComponentModel.DataAnnotations;

namespace EnterpriseIT.Application.DTOs.Ticket;

public class AssignTicketDto
{
    [Range(1, int.MaxValue)]
    public int AssignedToUserId { get; set; }
    public int? ExpectedAssignedToUserId { get; set; }
    public DateTimeOffset? ExpectedUpdatedAt { get; set; }
}

public class ClaimTicketDto
{
    public DateTimeOffset? ExpectedUpdatedAt { get; set; }
}
