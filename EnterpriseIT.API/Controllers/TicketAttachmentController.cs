using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using EnterpriseIT.API.Services;
using EnterpriseIT.Application.DTOs.TicketAttachment;
using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;

namespace EnterpriseIT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Technician,Employee")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TicketAttachmentController(
    ITicketAttachmentRepository attachments, ITicketRepository tickets,
    TicketAttachmentStorage storage, IMapper mapper, ILogger<TicketAttachmentController> logger) : ControllerBase
{
    private int? EmployeeUserId => User.IsInRole("Employee")
        ? int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) : null;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery, BindRequired, Range(1, int.MaxValue)] int ticketId)
    {
        if (await tickets.GetByIdWithDetailsAsync(ticketId, EmployeeUserId) == null) return NotFound();
        return Ok(mapper.Map<List<TicketAttachmentDto>>(await attachments.GetAllWithDetailsAsync(ticketId)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await attachments.GetByIdWithDetailsAsync(id, EmployeeUserId);
        return item == null ? NotFound() : Ok(mapper.Map<TicketAttachmentDto>(item));
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var item = await attachments.GetByIdWithDetailsAsync(id, EmployeeUserId);
        if (item == null) return NotFound();
        var stream = storage.OpenRead(item.FilePath);
        if (stream == null) return NotFound("Dosya depoda bulunamadı.");
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(stream, "application/octet-stream", item.FileName);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(TicketAttachmentStorage.MaxRequestSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = TicketAttachmentStorage.MaxFileSize)]
    [EnableRateLimiting("attachment-upload")]
    public async Task<IActionResult> Upload(
        [FromForm, Range(1, int.MaxValue)] int ticketId, [Required] IFormFile file,
        CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdWithDetailsAsync(ticketId, EmployeeUserId);
        if (ticket == null) return NotFound();
        if (ticket.ClosedAt != null || ticket.Status.StatusName == "Closed")
            return Conflict("Kapalı ticket’a dosya eklenemez.");

        (string Key, string FileName) saved;
        try { saved = await storage.SaveAsync(file, cancellationToken); }
        catch (InvalidDataException error) { return BadRequest(error.Message); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            logger.LogError(error, "Attachment could not be stored for ticket {TicketId}", ticketId);
            return Problem("Dosya saklanamadı. Daha sonra tekrar deneyin.");
        }

        var attachment = new TicketAttachment
        {
            TicketId = ticketId, UploadedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            FileName = saved.FileName, FilePath = saved.Key
        };
        try
        {
            if (!await attachments.TryAddToOpenTicketAsync(attachment, EmployeeUserId))
            {
                try { storage.DeleteUnreferenced(saved.Key); }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                { logger.LogWarning(error, "Unreferenced attachment remains in storage: {StorageKey}", saved.Key); }
                return Conflict("Ticket kapanmış veya değişmiş olabilir. Güncel bilgileri yükleyin.");
            }
        }
        catch (Exception error)
        {
            // A commit failure can have an uncertain outcome. Keep the file so a committed record never loses it.
            logger.LogError(error, "Attachment metadata save failed; storage key retained for reconciliation: {StorageKey}", saved.Key);
            return Problem("Dosya kaydı tamamlanamadı. Tekrar yüklemeden önce ek listesini yenileyin.");
        }
        return CreatedAtAction(nameof(GetById), new { id = attachment.AttachmentId }, new TicketAttachmentDto
        {
            AttachmentId = attachment.AttachmentId, TicketId = ticketId, TicketTitle = ticket.Title,
            FileName = attachment.FileName, UploadedBy = User.Identity?.Name ?? "", CreatedAt = attachment.CreatedAt
        });
    }
}
