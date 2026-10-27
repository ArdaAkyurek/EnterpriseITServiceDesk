using AutoMapper;
using EnterpriseIT.Application.DTOs.TicketComment;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EnterpriseIT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Technician,Employee")]


    public class TicketCommentController : ControllerBase
    {
        private readonly ITicketCommentRepository _ticketCommentRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private int? EmployeeUserId => User.IsInRole("Employee")
            ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) : null;

        public TicketCommentController(
            ITicketCommentRepository ticketCommentRepository,
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _ticketCommentRepository = ticketCommentRepository;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery, BindRequired, Range(1, int.MaxValue)] int ticketId)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(ticketId, EmployeeUserId);
            if (ticket == null) return NotFound();
            var comments =
                await _ticketCommentRepository.GetAllWithDetailsAsync(ticketId);

            return Ok(_mapper.Map<List<TicketCommentDto>>(comments));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment =
                await _ticketCommentRepository.GetByIdWithDetailsAsync(id, EmployeeUserId);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TicketCommentDto>(comment));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTicketCommentDto dto)
        {
            var ticket =
                await _ticketRepository.GetByIdWithDetailsAsync(dto.TicketId, EmployeeUserId);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.ClosedAt != null || ticket.Status.StatusName == "Closed")
                return Conflict("Kapalı ticket’a yeni yorum eklenemez.");

            var comment = _mapper.Map<TicketComment>(dto);
            comment.Comment = comment.Comment.Trim();
            comment.CreatedAt = DateTime.UtcNow;

            // Giriş yapan kullanıcı
            comment.UserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            if (!await _ticketCommentRepository.TryAddToOpenTicketAsync(comment, EmployeeUserId))
                return Conflict("Ticket kapanmış veya değişmiş olabilir. Güncel bilgileri yükleyin.");

            var createdComment =
                await _ticketCommentRepository.GetByIdWithDetailsAsync(comment.CommentId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = comment.CommentId },
                _mapper.Map<TicketCommentDto>(createdComment));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateTicketCommentDto dto)
        {
            var comment =
                await _ticketCommentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            _mapper.Map(dto, comment);

            _ticketCommentRepository.Update(comment);

            await _ticketCommentRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment =
                await _ticketCommentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            _ticketCommentRepository.Delete(comment);

            await _ticketCommentRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}
