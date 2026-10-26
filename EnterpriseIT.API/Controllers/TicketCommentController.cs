using AutoMapper;
using EnterpriseIT.Application.DTOs.TicketComment;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class TicketCommentController : ControllerBase
    {
        private readonly ITicketCommentRepository _ticketCommentRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

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
        public async Task<IActionResult> GetAll()
        {
            var comments =
                await _ticketCommentRepository.GetAllWithDetailsAsync();

            return Ok(_mapper.Map<List<TicketCommentDto>>(comments));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment =
                await _ticketCommentRepository.GetByIdWithDetailsAsync(id);

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
                await _ticketRepository.GetByIdAsync(dto.TicketId);

            if (ticket == null)
            {
                return BadRequest("Geçersiz Ticket.");
            }

            var comment = _mapper.Map<TicketComment>(dto);

            // Şimdilik giriş yapan kullanıcı
            comment.UserId = 1;

            await _ticketCommentRepository.AddAsync(comment);
            await _ticketCommentRepository.SaveChangesAsync();

            var createdComment =
                await _ticketCommentRepository.GetByIdWithDetailsAsync(comment.CommentId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = comment.CommentId },
                _mapper.Map<TicketCommentDto>(createdComment));
        }

        [HttpPut("{id}")]
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
