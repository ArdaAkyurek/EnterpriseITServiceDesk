using AutoMapper;
using EnterpriseIT.Application.DTOs.Ticket;
using EnterpriseIT.Application.DTOs.Ticket.EnterpriseIT.Application.DTOs.Ticket;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketCategoryRepository _categoryRepository;
        private readonly ITicketPriorityRepository _priorityRepository;
        private readonly ITicketStatusRepository _statusRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

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
        public async Task<IActionResult> GetAll()
        {
            var tickets = await _ticketRepository.GetAllWithDetailsAsync();

            return Ok(_mapper.Map<List<TicketDto>>(tickets));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TicketDto>(ticket));
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTicketDto dto)
        {
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
            }

            // DTO -> Entity
            var ticket = _mapper.Map<Ticket>(dto);

            // Giriş yapan kullanıcı
            ticket.CreatedByUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            // Henüz kimseye atanmadı
            ticket.AssignedToUserId = null;

            // Varsayılan durum = Open
            ticket.StatusId = 1;

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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTicketDto dto)
        {
            // Ticket var mı?
            var ticket = await _ticketRepository.GetByIdAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

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

            
            if (status.StatusName == "Closed")
            {
                ticket.ClosedAt = DateTime.UtcNow;
            }
            else
            {
                ticket.ClosedAt = null;
            }

            _ticketRepository.Update(ticket);

            await _ticketRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
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
