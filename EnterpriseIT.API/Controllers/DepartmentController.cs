using AutoMapper;
using EnterpriseIT.Application.DTOs.Department;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
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

    public float Mod(float a, float b)
    {
        if (a == 0 || b== 0) { 
                return 0;
    }
       float sonuc = a % b;
        if (sonuc > 100)
        {
            return 100;
        }
        return sonuc;
    }

    public float Faktoriyel(int f)
    {

        if (f < 0) { 
            return 0;
        }

        float sonuc = 1;

        
        for (int i = f; i > 0; i--)
        {
            sonuc *= i; 
        }
     

        return sonuc;
    }

    public string nothesapla(float vize,float final,int devamsızlık)
    {
        if (devamsızlık > 10)
        {
            return "FF";
        }
        double sonuc = 0;
        sonuc = (vize * 0.4) + (final * 0.6);
        return sonuc switch {
            >= 90 => "AA",
             _ => "FF"


        };

    }


}
