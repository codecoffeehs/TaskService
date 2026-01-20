using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Services;

namespace TaskService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TaskSharingController(TaskSharingService taskSharingService) : ControllerBase
    {
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("INvalid User Id");
            }
            return userId;
        }

        [HttpPost("invite/{taskId}")]
        public async Task<IActionResult> SendInvite(Guid taskId, [FromBody] ShareTaskDto dto)
        {
            var userId = GetUserId();
            await taskSharingService.SendTaskInviteAsync(userId, taskId, dto);
            return Ok("Shared Successfully");
        }
    }
}
