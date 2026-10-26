using AutoMapper;
using EnterpriseIT.Application.DTOs.TicketAttachment;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketAttachmentController : ControllerBase
    {
        private readonly ITicketAttachmentRepository _ticketAttachmentRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public TicketAttachmentController(
            ITicketAttachmentRepository ticketAttachmentRepository,
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _ticketAttachmentRepository = ticketAttachmentRepository;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var attachments = await _ticketAttachmentRepository.GetAllWithDetailsAsync();

            return Ok(_mapper.Map<List<TicketAttachmentDto>>(attachments));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var attachment = await _ticketAttachmentRepository.GetByIdWithDetailsAsync(id);

            if (attachment == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TicketAttachmentDto>(attachment));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTicketAttachmentDto dto)
        {
            var ticket = await _ticketRepository.GetByIdAsync(dto.TicketId);

            if (ticket == null)
            {
                return BadRequest("Geçersiz Ticket.");
            }

            var attachment = _mapper.Map<TicketAttachment>(dto);

            // Giriş yapan kullanıcı
            attachment.UploadedByUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            await _ticketAttachmentRepository.AddAsync(attachment);
            await _ticketAttachmentRepository.SaveChangesAsync();

            var createdAttachment =
                await _ticketAttachmentRepository.GetByIdWithDetailsAsync(attachment.AttachmentId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = attachment.AttachmentId },
                _mapper.Map<TicketAttachmentDto>(createdAttachment));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTicketAttachmentDto dto)
        {
            var attachment = await _ticketAttachmentRepository.GetByIdAsync(id);

            if (attachment == null)
            {
                return NotFound();
            }

            _mapper.Map(dto, attachment);

            _ticketAttachmentRepository.Update(attachment);

            await _ticketAttachmentRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var attachment = await _ticketAttachmentRepository.GetByIdAsync(id);

            if (attachment == null)
            {
                return NotFound();
            }

            _ticketAttachmentRepository.Delete(attachment);

            await _ticketAttachmentRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}
