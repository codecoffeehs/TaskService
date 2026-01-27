using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using MassTransit.Futures.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Services;

namespace TaskService.Controllers
{
    [Authorize(Policy = "UserPolicy", Roles = "TaskUser")]
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

        private string GetEmail()
        {
            var email = User.FindFirst("LinkedId")?.Value;

            if (string.IsNullOrWhiteSpace(email))
                throw new UnauthorizedException("Invalid Email");
            return email;
        }



        [HttpPost("invite/{taskId}")]
        public async Task<IActionResult> SendInvite(Guid taskId, [FromBody] ShareTaskDto dto)
        {
            var userId = GetUserId();
            var email = GetEmail();
            await taskSharingService.SendTaskInviteAsync(userId, taskId, email, dto);
            return Ok("Shared Successfully");
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetSharedTaskRequests()
        {
            var userId = GetUserId();
            var result = await taskSharingService.GetSharedTaskRequestsAsync(userId);
            return Ok(result);
        }

        [HttpPost("accept/{inviteId}")]
        public async Task<IActionResult> AcceptInviteAndCreateTask(Guid inviteId, [FromBody] CreateTaskDto dto)
        {
            var userId = GetUserId();
            var result = await taskSharingService.AcceptInviteAndCreateTask(userId, inviteId, dto);
            return Ok(result);
        }

        [HttpPost("reject/{inviteId}")]

        public async Task<IActionResult> RejectInvite(Guid inviteId)
        {
            var userId = GetUserId();
            await taskSharingService.RejectInvite(userId, inviteId);
            return Ok();
        }

    }
}
