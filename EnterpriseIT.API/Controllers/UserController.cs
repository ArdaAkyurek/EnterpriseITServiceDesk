using AutoMapper;
using EnterpriseIT.Application.DTOs.TicketStatus;

using EnterpriseIT.Application.DTOs.User;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public UserController(
    IUserRepository userRepository,
    IDepartmentRepository departmentRepository,
    IRoleRepository roleRepository,
    IMapper mapper)
        {
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        [HttpGet]

        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAllAsync();

            var userDtos = _mapper.Map<List<UserDto>>(users);

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if(user == null) 
            { 
                    return NotFound();
            }
            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);

        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

            if (existingUser != null)
            {
                return BadRequest("Bu email zaten kullanılıyor.");
            }
            var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);

            if (department == null)
            {
                return BadRequest("Geçersiz Department.");
            }
            var role = await _roleRepository.GetByIdAsync(dto.RoleId);

            if (role == null)
            {
                return BadRequest("Role bulunamadı.");
            }

            var user = _mapper.Map<User>(dto);
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return CreatedAtAction(
    nameof(GetById),
    new { id = user.UserId },
    _mapper.Map<UserDto>(user)
            );


        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

            if (existingUser != null && existingUser.UserId != id)
            {
                return BadRequest("Bu email zaten kullanılıyor.");
            }

            
            var department =
         await _departmentRepository.GetByIdAsync(dto.DepartmentId);

            if (department == null)
            {
                return BadRequest("Department bulunamadı.");
            }
            var role =
         await _roleRepository.GetByIdAsync(dto.RoleId);

            if (role == null)
            {
                return BadRequest("Role bulunamadı.");
            }

            _mapper.Map(dto, user);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();
            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            _userRepository.Delete(user);
            await _userRepository.SaveChangesAsync();
            return NoContent();


        }
    }
}
