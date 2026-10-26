using AutoMapper;
using EnterpriseIT.Application.DTOs.AssetType;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetTypeController : ControllerBase
    {
        private readonly IAssetTypeRepository _assetTypeRepository;
        private readonly IMapper _mapper;

        public AssetTypeController(
            IAssetTypeRepository assetTypeRepository,
            IMapper mapper)
        {
            _assetTypeRepository = assetTypeRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var assetTypes = await _assetTypeRepository.GetAllAsync();

            return Ok(_mapper.Map<List<AssetTypeDto>>(assetTypes));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var assetType = await _assetTypeRepository.GetByIdAsync(id);

            if (assetType == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AssetTypeDto>(assetType));
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateAssetTypeDto dto)
        {
            var existingAssetType =
                await _assetTypeRepository.GetByNameAsync(dto.Name);

            if (existingAssetType != null)
            {
                return BadRequest("Bu Asset Type zaten mevcut.");
            }

            var assetType = _mapper.Map<AssetType>(dto);

            await _assetTypeRepository.AddAsync(assetType);
            await _assetTypeRepository.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = assetType.AssetTypeId },
                _mapper.Map<AssetTypeDto>(assetType));
        }

        [HttpPut("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateAssetTypeDto dto)
        {
            var assetType = await _assetTypeRepository.GetByIdAsync(id);

            if (assetType == null)
            {
                return NotFound();
            }

            var existingAssetType =
                await _assetTypeRepository.GetByNameAsync(dto.Name);

            if (existingAssetType != null &&
                existingAssetType.AssetTypeId != id)
            {
                return BadRequest("Bu Asset Type zaten mevcut.");
            }

            _mapper.Map(dto, assetType);

            _assetTypeRepository.Update(assetType);

            await _assetTypeRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var assetType = await _assetTypeRepository.GetByIdAsync(id);

            if (assetType == null)
            {
                return NotFound();
            }

            _assetTypeRepository.Delete(assetType);

            await _assetTypeRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}
