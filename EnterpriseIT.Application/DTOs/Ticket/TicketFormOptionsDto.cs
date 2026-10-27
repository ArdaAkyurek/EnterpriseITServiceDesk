using EnterpriseIT.Application.DTOs.Asset;
using EnterpriseIT.Application.DTOs.TicketCategory;
using EnterpriseIT.Application.DTOs.TicketPriority;

namespace EnterpriseIT.Application.DTOs.Ticket;

public record TicketFormOptionsDto(
    List<TicketCategoryDto> Categories,
    List<TicketPriorityDto> Priorities,
    List<AssetDto> Assets);
