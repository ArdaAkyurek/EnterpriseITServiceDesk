using AutoMapper;
using EnterpriseIT.Application.DTOs.AssetMaintenance;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetMaintenanceController : ControllerBase
    {
        private readonly IAssetMaintenanceRepository _assetMaintenanceRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AssetMaintenanceController(
            IAssetMaintenanceRepository assetMaintenanceRepository,
            IAssetRepository assetRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _assetMaintenanceRepository = assetMaintenanceRepository;
            _assetRepository = assetRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var maintenances =
                await _assetMaintenanceRepository.GetAllWithDetailsAsync();

            return Ok(_mapper.Map<List<AssetMaintenanceDto>>(maintenances));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var maintenance =
                await _assetMaintenanceRepository.GetByIdWithDetailsAsync(id);

            if (maintenance == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AssetMaintenanceDto>(maintenance));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAssetMaintenanceDto dto)
        {
            var asset = await _assetRepository.GetByIdAsync(dto.AssetId);

            if (asset == null)
            {
                return BadRequest("Geçersiz Asset.");
            }

            var user = await _userRepository.GetByIdAsync(dto.PerformedByUserId);

            if (user == null)
            {
                return BadRequest("Geçersiz User.");
            }

            if (dto.Cost < 0)
            {
                return BadRequest("Bakım maliyeti negatif olamaz.");
            }

            var maintenance = _mapper.Map<AssetMaintenance>(dto);

            await _assetMaintenanceRepository.AddAsync(maintenance);
            await _assetMaintenanceRepository.SaveChangesAsync();

            var createdMaintenance =
                await _assetMaintenanceRepository.GetByIdWithDetailsAsync(maintenance.MaintenanceId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = maintenance.MaintenanceId },
                _mapper.Map<AssetMaintenanceDto>(createdMaintenance));
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAssetMaintenanceDto dto)
        {
            var maintenance = await _assetMaintenanceRepository.GetByIdAsync(id);

            if (maintenance == null)
            {
                return NotFound();
            }

            var asset = await _assetRepository.GetByIdAsync(dto.AssetId);

            if (asset == null)
            {
                return BadRequest("Geçersiz Asset.");
            }

            var user = await _userRepository.GetByIdAsync(dto.PerformedByUserId);

            if (user == null)
            {
                return BadRequest("Geçersiz User.");
            }

            if (dto.Cost < 0)
            {
                return BadRequest("Bakım maliyeti negatif olamaz.");
            }

            if (dto.MaintenanceDate > DateTime.UtcNow)
            {
                return BadRequest("Bakım tarihi gelecekte olamaz.");
            }

            _mapper.Map(dto, maintenance);

            _assetMaintenanceRepository.Update(maintenance);

            await _assetMaintenanceRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var maintenance = await _assetMaintenanceRepository.GetByIdAsync(id);

            if (maintenance == null)
            {
                return NotFound();
            }

            _assetMaintenanceRepository.Delete(maintenance);

            await _assetMaintenanceRepository.SaveChangesAsync();

            return NoContent();
        }


    }
}
