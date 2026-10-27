using AutoMapper;
using EnterpriseIT.Application.DTOs.Ticket;
using EnterpriseIT.Application.DTOs.Ticket.EnterpriseIT.Application.DTOs.Ticket;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EnterpriseIT.Application.DTOs.Asset;
using EnterpriseIT.Application.DTOs.TicketCategory;
using EnterpriseIT.Application.DTOs.TicketPriority;

namespace EnterpriseIT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Technician,Employee")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketCategoryRepository _categoryRepository;
        private readonly ITicketPriorityRepository _priorityRepository;
        private readonly ITicketStatusRepository _statusRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private int? EmployeeUserId => User.IsInRole("Employee")
            ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) : null;

        public TicketController(
            ITicketRepository ticketRepository,
            ITicketCategoryRepository categoryRepository,
            ITicketPriorityRepository priorityRepository,
            ITicketStatusRepository statusRepository,
            IAssetRepository assetRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _categoryRepository = categoryRepository;
            _priorityRepository = priorityRepository;
            _statusRepository = statusRepository;
            _assetRepository = assetRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? view = null)
        {
            view ??= User.IsInRole("Employee") ? "created" : "all";
            if (view is not ("all" or "assigned" or "unassigned" or "created"))
                return BadRequest("Geçersiz çalışma listesi.");
            if (User.IsInRole("Employee") && view != "created") return Forbid();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var tickets = await _ticketRepository.GetAllWithDetailsAsync(
                view == "created" ? userId : null,
                view == "assigned" ? userId : null,
                view == "unassigned");

            return Ok(_mapper.Map<List<TicketDto>>(tickets));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id, EmployeeUserId);

            if (ticket == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TicketDto>(ticket));
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTicketDto dto)
        {
            dto.Title = dto.Title.Trim();
            dto.Description = dto.Description.Trim();
            // Category kontrolü
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);

            if (category == null)
            {
                return BadRequest("Geçersiz Ticket Category.");
            }

            // Priority kontrolü
            var priority = await _priorityRepository.GetByIdAsync(dto.PriorityId);

            if (priority == null)
            {
                return BadRequest("Geçersiz Ticket Priority.");
            }

            // Asset kontrolü (opsiyonel)
            if (dto.AssetId != null)
            {
                var asset = await _assetRepository.GetByIdAsync(dto.AssetId.Value);

                if (asset == null)
                {
                    return BadRequest("Geçersiz Asset.");
                }

                if (EmployeeUserId.HasValue &&
                    !(await _assetRepository.GetForTicketAsync(EmployeeUserId)).Any(a => a.AssetId == dto.AssetId))
                {
                    return BadRequest("Yalnızca üzerinize zimmetli bir demirbaş seçebilirsiniz.");
                }
            }

            var openStatus = (await _statusRepository.GetAllAsync()).FirstOrDefault(s => s.StatusName == "Open");
            if (openStatus == null)
                return Problem("Open ticket durumu tanımlı değil. Yöneticiyle iletişime geçin.", statusCode: 503);

            // DTO -> Entity
            var ticket = _mapper.Map<Ticket>(dto);

            // Giriş yapan kullanıcı
            ticket.CreatedByUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            // Henüz kimseye atanmadı
            ticket.AssignedToUserId = null;

            // Varsayılan durum = Open
            ticket.StatusId = openStatus.TicketStatusId;
            ticket.CreatedAt = DateTime.UtcNow;

            await _ticketRepository.AddAsync(ticket);
            await _ticketRepository.SaveChangesAsync();

            // Navigation Property'lerle birlikte tekrar çekiyoruz
            var createdTicket =
                await _ticketRepository.GetByIdWithDetailsAsync(ticket.TicketId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = ticket.TicketId },
                _mapper.Map<TicketDto>(createdTicket));
        }

        [HttpGet("form-options")]
        public async Task<IActionResult> GetFormOptions()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var priorities = await _priorityRepository.GetAllAsync();
            var assets = await _assetRepository.GetForTicketAsync(EmployeeUserId);
            return Ok(new TicketFormOptionsDto(
                _mapper.Map<List<TicketCategoryDto>>(categories),
                _mapper.Map<List<TicketPriorityDto>>(priorities),
                _mapper.Map<List<AssetDto>>(assets)));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> Update(int id, UpdateTicketDto dto)
        {
            // Ticket var mı?
            var ticket = await _ticketRepository.GetByIdAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Technician") && ticket.AssignedToUserId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
                return Forbid();

            if (dto.AssignedToUserId != ticket.AssignedToUserId)
                return BadRequest("Atama değişikliği için teknisyene ata veya üzerime al işlemini kullanın.");

            if (dto.StatusId != ticket.StatusId)
                return BadRequest("Durum değişikliği için işe başla veya ticket kapat işlemini kullanın.");

            var expectedAssigneeId = ticket.AssignedToUserId;
            var expectedUpdatedAt = ticket.UpdatedAt;

            // Category kontrolü
            var category =
                await _categoryRepository.GetByIdAsync(dto.CategoryId);

            if (category == null)
            {
                return BadRequest("Geçersiz Ticket Category.");
            }

            // Priority kontrolü
            var priority =
                await _priorityRepository.GetByIdAsync(dto.PriorityId);

            if (priority == null)
            {
                return BadRequest("Geçersiz Ticket Priority.");
            }

            // Status kontrolü
            var status =
                await _statusRepository.GetByIdAsync(dto.StatusId);

            if (status == null)
            {
                return BadRequest("Geçersiz Ticket Status.");
            }

            
            if (dto.AssignedToUserId != null)
            {
                var assignedUser =
                    await _userRepository.GetByIdAsync(dto.AssignedToUserId.Value);

                if (assignedUser == null)
                {
                    return BadRequest("Geçersiz User.");
                }
            }

            
            if (dto.AssetId != null)
            {
                var asset =
                    await _assetRepository.GetByIdAsync(dto.AssetId.Value);

                if (asset == null)
                {
                    return BadRequest("Geçersiz Asset.");
                }
            }

            _mapper.Map(dto, ticket);

            
            if (!await _ticketRepository.TryUpdateDetailsAsync(ticket, expectedAssigneeId, expectedUpdatedAt))
                return Conflict("Ticket başka bir işlemle değiştirildi. Güncel bilgileri yükleyip tekrar deneyin.");

            return NoContent();
        }

        [HttpGet("assignment-options")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAssignmentOptions()
        {
            var technicians = await _userRepository.GetActiveTechniciansAsync();
            return Ok(technicians.Select(u => new TicketAssigneeDto(u.UserId, $"{u.FirstName} {u.LastName}")));
        }

        [HttpPut("{id:int}/assignment")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign(int id, AssignTicketDto dto)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id);
            if (ticket == null) return NotFound();
            if (ticket.ClosedAt != null || ticket.Status.StatusName == "Closed")
                return Conflict("Kapalı ticket için atama yapılamaz.");

            if (!(await _userRepository.GetActiveTechniciansAsync()).Any(u => u.UserId == dto.AssignedToUserId))
                return BadRequest("Aktif bir Technician kullanıcısı seçin.");

            if (!await _ticketRepository.TryAssignAsync(id, dto.AssignedToUserId, dto.ExpectedAssignedToUserId, dto.ExpectedUpdatedAt?.UtcDateTime))
                return Conflict("Ticket veya teknisyen bilgileri değişti. Güncel bilgileri yükleyip tekrar deneyin.");

            return Ok(_mapper.Map<TicketDto>(await _ticketRepository.GetByIdWithDetailsAsync(id)));
        }

        [HttpPost("{id:int}/claim")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> Claim(int id, ClaimTicketDto dto)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id);
            if (ticket == null) return NotFound();
            if (ticket.AssignedToUserId != null || ticket.ClosedAt != null || ticket.Status.StatusName == "Closed")
                return Conflict("Bu ticket atanmış veya kapatılmış. Üzerinize alamazsınız.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _ticketRepository.TryAssignAsync(id, userId, null, dto.ExpectedUpdatedAt?.UtcDateTime))
                return Conflict("Ticket başka bir işlemle değiştirildi. Güncel bilgileri yükleyip tekrar deneyin.");

            return Ok(_mapper.Map<TicketDto>(await _ticketRepository.GetByIdWithDetailsAsync(id)));
        }

        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeTicketStatusDto dto)
        {
            if (dto.Status is not ("In Progress" or "Closed")) return BadRequest("Geçersiz hedef durum.");
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id);
            if (ticket == null) return NotFound();
            int? technicianId = User.IsInRole("Technician") ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) : null;
            if (technicianId.HasValue && ticket.AssignedToUserId != technicianId) return Forbid();
            if (ticket.ClosedAt != null || ticket.Status.StatusName == "Closed")
                return Conflict("Kapalı ticket için durum değiştirilemez.");
            if (dto.Status == "In Progress" && ticket.Status.StatusName != "Open" ||
                dto.Status == "Closed" && ticket.Status.StatusName != "In Progress")
                return Conflict("Önce işe başlayın, ardından ticket’ı kapatın.");
            if (ticket.AssignedToUserId == null)
                return Conflict("İşe başlamadan önce ticket’ı aktif bir teknisyene atayın.");
            if (dto.Status == "In Progress" && !(await _userRepository.GetActiveTechniciansAsync()).Any(u => u.UserId == ticket.AssignedToUserId))
                return Conflict("İşe başlamadan önce aktif bir teknisyen atayın.");
            var status = (await _statusRepository.GetAllAsync()).FirstOrDefault(s => s.StatusName == dto.Status);
            if (status == null) return Problem("Hedef ticket durumu tanımlı değil. Yöneticiyle iletişime geçin.", statusCode: 503);
            if (!await _ticketRepository.TryChangeStatusAsync(id, status.TicketStatusId, dto.ExpectedStatus,
                    dto.ExpectedAssignedToUserId, dto.ExpectedUpdatedAt?.UtcDateTime, technicianId))
                return Conflict("Ticket başka bir işlemle değiştirildi. Güncel bilgileri yükleyip tekrar deneyin.");
            return Ok(_mapper.Map<TicketDto>(await _ticketRepository.GetByIdWithDetailsAsync(id)));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            _ticketRepository.Delete(ticket);

            await _ticketRepository.SaveChangesAsync();

            return NoContent();
        }


    }
}
