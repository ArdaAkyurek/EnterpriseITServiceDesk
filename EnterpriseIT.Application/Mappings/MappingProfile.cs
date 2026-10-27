using AutoMapper;   
using EnterpriseIT.Application.DTOs.Asset;
using EnterpriseIT.Application.DTOs.AssetAssignment;
using EnterpriseIT.Application.DTOs.AssetAssignmentDto;
using EnterpriseIT.Application.DTOs.AssetMaintenance;
using EnterpriseIT.Application.DTOs.AssetType;
using EnterpriseIT.Application.DTOs.AuditLog;
using EnterpriseIT.Application.DTOs.Department;
using EnterpriseIT.Application.DTOs.Notification;
using EnterpriseIT.Application.DTOs.Role;
using EnterpriseIT.Application.DTOs.Ticket;
using EnterpriseIT.Application.DTOs.Ticket.EnterpriseIT.Application.DTOs.Ticket;
using EnterpriseIT.Application.DTOs.TicketAttachment;
using EnterpriseIT.Application.DTOs.TicketCategory;
using EnterpriseIT.Application.DTOs.TicketComment;
using EnterpriseIT.Application.DTOs.TicketPriority;
using EnterpriseIT.Application.DTOs.TicketStatus;
using EnterpriseIT.Application.DTOs.User;
using EnterpriseIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EnterpriseIT.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Department, DepartmentDto>();

        CreateMap<CreateDepartmentDto, Department>();

        CreateMap<UpdateDepartmentDto, Department>();

        CreateMap<Role, RoleDto>();

        CreateMap<CreateRoleDto, Role>();

        CreateMap<UpdateRoleDto, Role>();

        CreateMap<TicketPriority, TicketPriorityDto>();

        CreateMap<CreateTicketPriorityDto, TicketPriority>();

        CreateMap<UpdateTicketPriorityDto, TicketPriority>();

        CreateMap<TicketStatus, TicketStatusDto>();

        CreateMap<CreateTicketStatusDto, TicketStatus>();

        CreateMap<UpdateTicketStatusDto, TicketStatus>();

        CreateMap<TicketCategory,TicketCategoryDto>()
            .ForMember(dest => dest.TicketCategoryName, opt => opt.MapFrom(src => src.CategoryName));

        CreateMap<CreateTicketCategoryDto, TicketCategory>();

        CreateMap<UpdateTicketCategoryDto, TicketCategory>();

        CreateMap<User, UserDto>();

        CreateMap<CreateUserDto, User>();

        CreateMap<UpdateUserDto, User>();

        CreateMap<Ticket, TicketDto>()
    .ForMember(dest => dest.CreatedBy,
        opt => opt.MapFrom(src =>
            src.CreatedByUser.FirstName + " " + src.CreatedByUser.LastName))

    .ForMember(dest => dest.AssignedTo,
        opt => opt.MapFrom(src =>
            src.AssignedToUser == null
                ? null
                : src.AssignedToUser.FirstName + " " + src.AssignedToUser.LastName))

    .ForMember(dest => dest.Priority,
        opt => opt.MapFrom(src => src.Priority.PriorityName))

    .ForMember(dest => dest.Status,
        opt => opt.MapFrom(src => src.Status.StatusName))

    .ForMember(dest => dest.Category,
        opt => opt.MapFrom(src => src.Category.CategoryName))

    .ForMember(
    dest => dest.Asset,
    opt => opt.MapFrom(src =>
        src.Asset == null
            ? null
            : $"{src.Asset.AssetCode} - {src.Asset.Brand} {src.Asset.Model}"));

        CreateMap<CreateTicketDto, Ticket>();

        CreateMap<UpdateTicketDto, Ticket>();

        CreateMap<Asset, AssetDto>();

        CreateMap<CreateAssetDto, Asset>();

        CreateMap<UpdateAssetDto, Asset>();

        CreateMap<AssetType, AssetTypeDto>();

        CreateMap<CreateAssetTypeDto, AssetType>();

        CreateMap<UpdateAssetTypeDto, AssetType>();

        CreateMap<TicketComment, TicketCommentDto>()
    .ForMember(
        dest => dest.User,
        opt => opt.MapFrom(src =>
            $"{src.User.FirstName} {src.User.LastName}"
        ))
    .ForMember(
    dest => dest.TicketTitle,
    opt => opt.MapFrom(src => src.Ticket.Title));

        CreateMap<CreateTicketCommentDto, TicketComment>();

        CreateMap<UpdateTicketCommentDto, TicketComment>();

        CreateMap<TicketAttachment, TicketAttachmentDto>()
    .ForMember(dest => dest.TicketTitle,
        opt => opt.MapFrom(src => src.Ticket.Title))

    .ForMember(dest => dest.UploadedBy,
        opt => opt.MapFrom(src =>
            $"{src.UploadedByUser.FirstName} {src.UploadedByUser.LastName}"));

        CreateMap<CreateTicketAttachmentDto, TicketAttachment>();

        CreateMap<UpdateTicketAttachmentDto, TicketAttachment>();

        CreateMap<Notification, NotificationDto>()
    .ForMember(dest => dest.User,
        opt => opt.MapFrom(src =>
            $"{src.User.FirstName} {src.User.LastName}"));

        CreateMap<CreateNotificationDto, Notification>();

        CreateMap<UpdateNotificationDto, Notification>();

        CreateMap<AssetAssignment, AssetAssignmentDto>()
    .ForMember(dest => dest.User,
        opt => opt.MapFrom(src =>
            $"{src.User.FirstName} {src.User.LastName}"))

    .ForMember(dest => dest.Asset,
        opt => opt.MapFrom(src =>
            $"{src.Asset.AssetCode} - {src.Asset.Brand} {src.Asset.Model}"));

        CreateMap<CreateAssetAssignmentDto, AssetAssignment>();

        CreateMap<UpdateAssetAssignmentDto, AssetAssignment>();

        CreateMap<AssetMaintenance, AssetMaintenanceDto>()
    .ForMember(dest => dest.Asset,
        opt => opt.MapFrom(src =>
            $"{src.Asset.AssetCode} - {src.Asset.Brand} {src.Asset.Model}"))

    .ForMember(dest => dest.PerformedBy,
        opt => opt.MapFrom(src =>
            $"{src.PerformedByUser.FirstName} {src.PerformedByUser.LastName}"));

        CreateMap<CreateAssetMaintenanceDto, AssetMaintenance>();

        CreateMap<UpdateAssetMaintenanceDto, AssetMaintenance>();

        CreateMap<AuditLog, AuditLogDto>()
    .ForMember(dest => dest.User,
        opt => opt.MapFrom(src =>
            $"{src.User.FirstName} {src.User.LastName}"));
    }
}
