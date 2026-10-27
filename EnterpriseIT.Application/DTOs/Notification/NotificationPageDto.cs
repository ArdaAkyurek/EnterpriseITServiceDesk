namespace EnterpriseIT.Application.DTOs.Notification;

public class NotificationPageDto
{
    public List<NotificationDto> Items { get; set; } = [];
    public int Page { get; set; }
    public bool HasMore { get; set; }
    public int UnreadCount { get; set; }
}
