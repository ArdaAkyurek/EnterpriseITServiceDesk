using AutoMapper;
using EnterpriseIT.Application.DTOs.Department;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;

    public DepartmentController(
        IDepartmentRepository departmentRepository,
        IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _departmentRepository.GetAllAsync();

        var departmentDtos = _mapper.Map<List<DepartmentDto>>(departments);

        return Ok(departmentDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department == null)
        {
            return NotFound();
        }

        var departmentDto = _mapper.Map<DepartmentDto>(department);

        return Ok(departmentDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        var department = _mapper.Map<Department>(dto);

        await _departmentRepository.AddAsync(department);
        await _departmentRepository.SaveChangesAsync();

        var departmentDto = _mapper.Map<DepartmentDto>(department);

        return CreatedAtAction(
            nameof(GetById),
            new { id = department.DepartmentId },
            departmentDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateDepartmentDto dto)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department == null)
        {
            return NotFound();
        }

        _mapper.Map(dto, department);

        _departmentRepository.Update(department);
        await _departmentRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department == null)
        {
            return NotFound();
        }

        _departmentRepository.Delete(department);
        await _departmentRepository.SaveChangesAsync();

        return NoContent();
    }
}