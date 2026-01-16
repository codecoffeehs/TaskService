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
    public class TaskCollabController(TaskCollabService taskCollabService) : ControllerBase
    {
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Not Allowed");
            }

            return userId;
        }
        [HttpPost("invite/{taskId}")]
        public async Task<IActionResult> TaskInvite(Guid taskId, [FromBody] InviteTaskDto dto)
        {
            var userId = GetUserId();
            await taskCollabService.TaskInviteAsync(userId, taskId, dto);
            return Ok("Invite Sent");
        }

        [HttpPost("accept/{taskId}")]
        public async Task<IActionResult> AcceptInvite(Guid taskId)
        {
            var userId = GetUserId();
            await taskCollabService.AcceptInviteAsync(userId, taskId);
            return Ok("Invite Accepted");
        }

        [HttpPost("reject/{taskId}")]
        public async Task<IActionResult> RejectInvite(Guid taskId)
        {
            var userId = GetUserId();
            await taskCollabService.RejectInviteAsync(userId, taskId);
            return Ok("Invite Rejected");
        }

        [HttpPost("cancel/{taskId}")]
        public async Task<IActionResult> CancelInvite(Guid taskId, [FromBody] InviteTaskDto dto)
        {
            var userId = GetUserId();
            await taskCollabService.CancelTaskInviteAsync(userId, taskId, dto);
            return Ok("Invite Cancelled");
        }

        [HttpGet("shared")]
        public async Task<IActionResult> GetSharedTasks()
        {
            var userId = GetUserId();
            var results = await taskCollabService.GetSharedTasksAsync(userId);
            return Ok(results);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetSharedRequests()
        {
            var userId = GetUserId();
            var results = await taskCollabService.GetSharedTaskRequestsAsync(userId);
            return Ok(results);
        }
    }
}
