using AutoMapper;
using EnterpriseIT.Application.DTOs.AuditLog;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseIT.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public AuditLogController(
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery, Range(1, 1000000)] int page = 1)
    {
        var logs = await _auditLogRepository.GetAllWithDetailsAsync(page);

        return Ok(new AuditLogPageDto(_mapper.Map<List<AuditLogDto>>(logs.Take(AuditLogPageDto.PageSize)), page, logs.Count > AuditLogPageDto.PageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var log = await _auditLogRepository.GetByIdWithDetailsAsync(id);

        if (log == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AuditLogDto>(log));
    }

}
