using AutoMapper;
using EnterpriseIT.Application.DTOs.AuditLog;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> GetAll()
    {
        var logs = await _auditLogRepository.GetAllWithDetailsAsync();

        return Ok(_mapper.Map<List<AuditLogDto>>(logs));
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

    [HttpPost]

    public async Task<IActionResult> Create(AuditLogDto dto)
    {
        var log = _mapper.Map<AuditLog>(dto);

        await _auditLogRepository.AddAsync(log);
        await _auditLogRepository.SaveChangesAsync();

        var AuditLogDto = _mapper.Map<AuditLogDto>(log);

        return Ok();

    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id,AuditLogDto dto)
    {
        var log = await _auditLogRepository.GetByIdAsync(id);
        if (log == null)
        {
            return NotFound();
        }
        log.Action = dto.Action;
        log.EntityName = dto.EntityName;
        log.EntityId = dto.EntityId;
        log.UpdatedAt = DateTime.UtcNow;

        _auditLogRepository.Update(log);
        await _auditLogRepository.SaveChangesAsync();

        return NoContent();

    }
}
