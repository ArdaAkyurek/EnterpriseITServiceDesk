using AutoMapper;
using EnterpriseIT.Application.DTOs.Asset;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetController : ControllerBase
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IAssetTypeRepository _assetTypeRepository;
        private readonly IMapper _mapper;

        public AssetController(
            IAssetRepository assetRepository,
            IAssetTypeRepository assetTypeRepository,
            IMapper mapper)
        {
            _assetRepository = assetRepository;
            _assetTypeRepository = assetTypeRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var assets = await _assetRepository.GetAllAsync();

            return Ok(_mapper.Map<List<AssetDto>>(assets));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AssetDto>(asset));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAssetDto dto)
        {
            if (dto.PurchaseDate == default || dto.WarrantyEndDate == default ||
                dto.WarrantyEndDate < dto.PurchaseDate)
            {
                return BadRequest("Satın alma ve garanti tarihlerini kontrol edin. Garanti bitişi satın alma tarihinden önce olamaz.");
            }

            dto.AssetCode = dto.AssetCode.Trim();
            dto.Brand = dto.Brand.Trim();
            dto.Model = dto.Model.Trim();
            dto.SerialNumber = dto.SerialNumber.Trim();
            dto.Status = dto.Status.Trim();

            var existingAsset =
                await _assetRepository.GetByAssetCodeAsync(dto.AssetCode);

            if (existingAsset != null)
            {
                return BadRequest("Bu Asset Code zaten kullanılıyor.");
            }

            var assetType =
                await _assetTypeRepository.GetByIdAsync(dto.AssetTypeId);

            if (assetType == null)
            {
                return BadRequest("Geçersiz Asset Type.");
            }

            var asset = _mapper.Map<Asset>(dto);
            asset.CreatedAt = DateTime.UtcNow;

            await _assetRepository.AddAsync(asset);
            await _assetRepository.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = asset.AssetId },
                _mapper.Map<AssetDto>(asset));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAssetDto dto)
        {
            if (dto.PurchaseDate == default || dto.WarrantyEndDate == default || dto.WarrantyEndDate < dto.PurchaseDate)
                return BadRequest("Satın alma ve garanti tarihlerini kontrol edin. Garanti bitişi satın alma tarihinden önce olamaz.");

            dto.AssetCode = dto.AssetCode.Trim();
            dto.Brand = dto.Brand.Trim();
            dto.Model = dto.Model.Trim();
            dto.SerialNumber = dto.SerialNumber.Trim();
            dto.Status = dto.Status.Trim();

            var asset = await _assetRepository.GetByIdAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            var existingAsset =
                await _assetRepository.GetByAssetCodeAsync(dto.AssetCode);

            if (existingAsset != null && existingAsset.AssetId != id)
            {
                return BadRequest("Bu Asset Code zaten kullanılıyor.");
            }

            var assetType =
                await _assetTypeRepository.GetByIdAsync(dto.AssetTypeId);

            if (assetType == null)
            {
                return BadRequest("Geçersiz Asset Type.");
            }

            _mapper.Map(dto, asset);
            asset.UpdatedAt = DateTime.UtcNow;

            _assetRepository.Update(asset);

            await _assetRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);

            if (asset == null)
            {
                return NotFound();
            }

            _assetRepository.Delete(asset);

            await _assetRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}
