using AutoMapper;
using EnterpriseIT.Application.DTOs.TicketStatus;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;


namespace EnterpriseIT.API.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketStatusController : ControllerBase
    {
        private readonly ITicketStatusRepository _ticketStatusRepository;
        private readonly IMapper _mapper;

        public TicketStatusController(
            ITicketStatusRepository ticketStatusRepository,
            IMapper mapper)
        {
            _ticketStatusRepository = ticketStatusRepository;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ticketStatuses = await _ticketStatusRepository.GetAllAsync();

            var ticketStatusDtos = _mapper.Map<List<TicketStatusDto>>(ticketStatuses);

            return Ok(ticketStatusDtos);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ticketStatus = await _ticketStatusRepository.GetByIdAsync(id);

            if (ticketStatus == null)
            {
                return NotFound();
            }

            var ticketStatusDto = _mapper.Map<TicketStatusDto>(ticketStatus);

            return Ok(ticketStatusDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTicketStatusDto dto)
        {
            var ticketStatus = _mapper.Map<TicketStatus>(dto);

            await _ticketStatusRepository.AddAsync(ticketStatus);
            await _ticketStatusRepository.SaveChangesAsync();

            var ticketStatusDto = _mapper.Map<TicketStatusDto>(ticketStatus);

            return CreatedAtAction(
                nameof(GetById),
                new { id = ticketStatus.TicketStatusId },
                ticketStatusDto);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTicketStatusDto dto)
        {
            var ticketStatus = await _ticketStatusRepository.GetByIdAsync(id);

            if (ticketStatus == null)
            {
                return NotFound();
            }

            _mapper.Map(dto, ticketStatus);

            _ticketStatusRepository.Update(ticketStatus);
            await _ticketStatusRepository.SaveChangesAsync();

            return NoContent();
        }

    }
}
