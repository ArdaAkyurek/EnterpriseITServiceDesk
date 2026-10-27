namespace EnterpriseIT.Application.DTOs.Ticket;

public class DashboardDto
{
    public string Scope { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Open { get; set; }
    public int InProgress { get; set; }
    public int Closed { get; set; }
    public int Other { get; set; }
    public int? UnassignedActiveCount { get; set; }
    public List<DashboardPriorityDto> ActiveByPriority { get; set; } = [];
    public List<DashboardTicketDto> RecentTickets { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}

public class DashboardPriorityDto
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardTicketDto
{
    public int TicketId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
}
