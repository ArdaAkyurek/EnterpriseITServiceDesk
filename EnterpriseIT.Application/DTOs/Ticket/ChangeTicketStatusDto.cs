using System.ComponentModel.DataAnnotations;

namespace EnterpriseIT.Application.DTOs.Ticket;

public class ChangeTicketStatusDto
{
    [Required, RegularExpression("^(In Progress|Closed)$")]
    public string Status { get; set; } = string.Empty;

    [Required, RegularExpression("^(Open|In Progress|Closed)$")]
    public string ExpectedStatus { get; set; } = string.Empty;

    public int? ExpectedAssignedToUserId { get; set; }
    public DateTimeOffset? ExpectedUpdatedAt { get; set; }
}
