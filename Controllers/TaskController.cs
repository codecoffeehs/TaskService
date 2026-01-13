using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Services;

namespace TaskService.Controllers;

    [Authorize(Policy = "UserPolicy", Roles = "TaskUser")]
    [Route("[controller]")]
    [ApiController]   
    public class TaskController(AllTasksService allTasksService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Not Allowed");
            }
            var result = await allTasksService.GetAllTasksAsync(userId);
            return Ok(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentTasks()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Not Allowed");
            }
            var result = await allTasksService.GetRecentTasksAsync(userId);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Not Allowed");
            }
            var result = await allTasksService.CreateTaskAsync(userId,createTaskDto);
            return Ok(result);
        }

        [HttpPatch("toggle/{taskId}")]
        public async Task<IActionResult> ToggleTask(Guid taskId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Not Allowed");
            }
            var result = await allTasksService.ToggleTaskAsync(userId,taskId);
            return Ok(result);
        }

        [HttpDelete("delete/{taskId}")]
        public async Task<IActionResult> DeleteTask(Guid taskId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Not Allowed");
            }
            var result = await allTasksService.DeleteTaskAsync(userId,taskId);
            return Ok(result);
        }
    }

