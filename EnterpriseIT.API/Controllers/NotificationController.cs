using AutoMapper;
using EnterpriseIT.Application.DTOs.Notification;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public NotificationController(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var notifications =
                await _notificationRepository.GetAllWithDetailsAsync();

            return Ok(_mapper.Map<List<NotificationDto>>(notifications));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var notification =
                await _notificationRepository.GetByIdWithDetailsAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<NotificationDto>(notification));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateNotificationDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
            {
                return BadRequest("Geçersiz User.");
            }

            var notification = _mapper.Map<Notification>(dto);

            notification.IsRead = false;

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            var createdNotification =
                await _notificationRepository.GetByIdWithDetailsAsync(notification.NotificationId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = notification.NotificationId },
                _mapper.Map<NotificationDto>(createdNotification));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateNotificationDto dto)
        {
            var notification =
                await _notificationRepository.GetByIdAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = dto.IsRead;

            _notificationRepository.Update(notification);

            await _notificationRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var notification =
                await _notificationRepository.GetByIdAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            _notificationRepository.Delete(notification);

            await _notificationRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}
