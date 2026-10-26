using AutoMapper;
using EnterpriseIT.Application.DTOs.TicketStatus;

using EnterpriseIT.Application.DTOs.User;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using EnterpriseIT.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace EnterpriseIT.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserController(
    IUserRepository userRepository,
    IDepartmentRepository departmentRepository,
    IRoleRepository roleRepository,
    IMapper mapper,
    IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
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
            dto.Email = dto.Email.Trim().ToLowerInvariant();
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
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            user.CreatedAt = DateTime.UtcNow;
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
            dto.Email = dto.Email.Trim().ToLowerInvariant();
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
            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            }
            user.UpdatedAt = DateTime.UtcNow;
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
