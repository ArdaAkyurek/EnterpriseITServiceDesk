using AutoMapper;
using EnterpriseIT.Application.DTOs.Role;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;


namespace EnterpriseIT.API.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        

        public RoleController(
            IRoleRepository roleRepository , IMapper mapper
            )
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Getall()
        {
            var roles = await _roleRepository.GetAllAsync();

            var roleDtos = _mapper.Map<List<RoleDto>>( roles );

            return Ok( roleDtos );
                
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var roleDto = _mapper.Map<RoleDto>(role);

            return Ok(roleDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleDto dto)
        {
            var role = _mapper.Map<Role>(dto);

            await _roleRepository.AddAsync(role);
            await _roleRepository.SaveChangesAsync();

            var roleDto = _mapper.Map<RoleDto>(role);

            return CreatedAtAction(
                nameof(GetById),
                new { id = role.RoleId },
                roleDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateRoleDto dto)
        {
            var role = await _roleRepository.GetByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            _mapper.Map(dto, role);

            _roleRepository.Update(role);
            await _roleRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            _roleRepository.Delete(role);
            await _roleRepository.SaveChangesAsync();

            return NoContent();
        }


    }
}
