using AutoMapper;
using EnterpriseIT.Application.DTOs.TicketCategory;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketCategoryController : ControllerBase
{
    private readonly ITicketCategoryRepository _ticketCategoryRepository;
    private readonly IMapper _mapper;

    public TicketCategoryController(
        ITicketCategoryRepository ticketCategoryRepository,
        IMapper mapper)
    {
        _ticketCategoryRepository = ticketCategoryRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ticketCategories = await _ticketCategoryRepository.GetAllAsync();

        var ticketCategoryDtos = _mapper.Map<List<TicketCategoryDto>>(ticketCategories);

        return Ok(ticketCategoryDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ticketCategory = await _ticketCategoryRepository.GetByIdAsync(id);

        if (ticketCategory == null)
        {
            return NotFound();
        }

        var ticketCategoryDto = _mapper.Map<TicketCategoryDto>(ticketCategory);

        return Ok(ticketCategoryDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketCategoryDto dto)
    {
        var ticketCategory = _mapper.Map<TicketCategory>(dto);

        await _ticketCategoryRepository.AddAsync(ticketCategory);
        await _ticketCategoryRepository.SaveChangesAsync();

        var ticketCategoryDto = _mapper.Map<TicketCategoryDto>(ticketCategory);

        return CreatedAtAction(
            nameof(GetById),
            new { id = ticketCategory.TicketCategoryId },
            ticketCategoryDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTicketCategoryDto dto)
    {
        var ticketCategory = await _ticketCategoryRepository.GetByIdAsync(id);

        if (ticketCategory == null)
        {
            return NotFound();
        }

        _mapper.Map(dto, ticketCategory);

        _ticketCategoryRepository.Update(ticketCategory);
        await _ticketCategoryRepository.SaveChangesAsync();

        return NoContent();
    }
}