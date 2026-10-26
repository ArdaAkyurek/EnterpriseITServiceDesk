using AutoMapper;
using EnterpriseIT.Application.DTOs.AssetAssignment;
using EnterpriseIT.Application.DTOs.AssetAssignmentDto;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AssetAssignmentController : ControllerBase
{
    private readonly IAssetAssignmentRepository _assetAssignmentRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public AssetAssignmentController(
        IAssetAssignmentRepository assetAssignmentRepository,
        IAssetRepository assetRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _assetAssignmentRepository = assetAssignmentRepository;
        _assetRepository = assetRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var assignments =
            await _assetAssignmentRepository.GetAllWithDetailsAsync();

        return Ok(_mapper.Map<List<AssetAssignmentDto>>(assignments));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var assignment =
            await _assetAssignmentRepository.GetByIdWithDetailsAsync(id);

        if (assignment == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AssetAssignmentDto>(assignment));
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync(CreateAssetAssignmentDto dto)
    {
        var asset = await _assetRepository.GetByIdAsync(dto.AssetId);

        if (asset == null)
        {
            return BadRequest("Geçersiz Asset.");

        }

        var user = await _userRepository.GetByIdAsync(dto.UserId);

        if (user == null)
        {
            return BadRequest("Geçersiz User.");
        }

        var activeAssignment =
    await _assetAssignmentRepository
        .GetActiveAssignmentByAssetIdAsync(dto.AssetId);

        if (activeAssignment != null)
        {
            return BadRequest("Bu Asset zaten başka bir kullanıcıya atanmış.");
        }

        if (dto.AssignedDate > DateTime.UtcNow.AddDays(1))
        {
            return BadRequest("AssignedDate geçersiz.");
        }

        var assignment = _mapper.Map<AssetAssignment>(dto);

        assignment.ReturnedDate = null;

        await _assetAssignmentRepository.AddAsync(assignment);

        await _assetAssignmentRepository.SaveChangesAsync();

        var createdAssignment =
            await _assetAssignmentRepository
                .GetByIdWithDetailsAsync(assignment.AssetAssignmentId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = assignment.AssetAssignmentId },
            _mapper.Map<AssetAssignmentDto>(createdAssignment));
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateAssetAssignmentDto dto)
    {
        var assignment =
            await _assetAssignmentRepository.GetByIdAsync(id);

        if (assignment == null)
        {
            return NotFound();
        }

        var asset =
            await _assetRepository.GetByIdAsync(dto.AssetId);

        if (asset == null)
        {
            return BadRequest("Geçersiz Asset.");
        }

        var user =
            await _userRepository.GetByIdAsync(dto.UserId);

        if (user == null)
        {
            return BadRequest("Geçersiz User.");
        }

        var activeAssignment =
            await _assetAssignmentRepository
                .GetActiveAssignmentByAssetIdAsync(dto.AssetId);

        if (activeAssignment != null &&
            activeAssignment.AssetAssignmentId != id)
        {
            return BadRequest("Bu Asset başka bir kullanıcıya atanmış.");
        }

        if (dto.ReturnedDate != null &&
            dto.AssignedDate > dto.ReturnedDate)
        {
            return BadRequest("ReturnedDate, AssignedDate'den önce olamaz.");
        }

        _mapper.Map(dto, assignment);

        _assetAssignmentRepository.Update(assignment);

        await _assetAssignmentRepository.SaveChangesAsync();

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var assignment =
            await _assetAssignmentRepository.GetByIdAsync(id);

        if (assignment == null)
        {
            return NotFound();
        }

        _assetAssignmentRepository.Delete(assignment);

        await _assetAssignmentRepository.SaveChangesAsync();

        return NoContent();
    }
}