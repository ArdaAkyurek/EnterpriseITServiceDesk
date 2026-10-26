using AutoMapper;
using EnterpriseIT.Application.DTOs.Department;
using EnterpriseIT.Application.DTOs.TicketPriority;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketPriorityController : ControllerBase
    {
        private readonly ITicketPriorityRepository _ticketPriorityRepository;
        private readonly IMapper _mapper;

        public TicketPriorityController(
            ITicketPriorityRepository ticketPriorityRepository,
            IMapper mapper)
        {
            _ticketPriorityRepository = ticketPriorityRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ticketpriorities = await _ticketPriorityRepository.GetAllAsync();

            var ticketpriorityDto = _mapper.Map<List<TicketPriorityDto>>(ticketpriorities);

            return Ok(ticketpriorityDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ticketpriority = await _ticketPriorityRepository.GetByIdAsync(id);

            if (ticketpriority == null)
            {
                return NotFound();
            }

            var ticketpriorityDto = _mapper.Map<TicketPriorityDto>(ticketpriority);

            return Ok(ticketpriorityDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTicketPriorityDto dto)
        {
            var ticketpriority = _mapper.Map<TicketPriority>(dto);

            await _ticketPriorityRepository.AddAsync(ticketpriority);
            await _ticketPriorityRepository.SaveChangesAsync();

            var ticketpriorityDto = _mapper.Map<TicketPriorityDto>(ticketpriority);

            return CreatedAtAction(
                nameof(GetById),
                new { id = ticketpriority.TicketPriorityId },
                ticketpriorityDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTicketPriorityDto dto)
        {
            var ticketpriority = await _ticketPriorityRepository.GetByIdAsync(id);

            if (ticketpriority == null)
            {
                return NotFound();
            }

            _mapper.Map(dto, ticketpriority);

            _ticketPriorityRepository.Update(ticketpriority);
            await _ticketPriorityRepository.SaveChangesAsync();

            return NoContent();
        }


    }
}
