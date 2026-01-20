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
    // ✅ GET: /Task
    // Returns all incomplete tasks of logged-in user
    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.GetAllTasksAsync(userId);
        return Ok(result);
    }

    // ✅ GET: /Task/recent
    // Returns RecentTasksDto containing Today + Upcoming + Overdue (Top 5 each)
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentTasks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.GetRecentTasksAsync(userId);
        return Ok(result);
    }

    // ✅ GET: /Task/today
    // Returns only tasks due today
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayTasks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.GetTodayTasksAsync(userId);
        return Ok(result);
    }

    // ✅ GET: /Task/upcoming
    // Returns only upcoming tasks (tomorrow and later)
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingTasks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.GetUpcomingTasksAsync(userId);
        return Ok(result);
    }

    // ✅ GET: /Task/overdue
    // Returns only overdue tasks (due date < today)
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueTasks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.GetOverdueTasksAsync(userId);
        return Ok(result);
    }


    // ✅ GET: /Task/nodue
    // Returns only nodue tasks
    public async Task<IActionResult> GetNodueTasks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.GetNodueTasksAsync(userId);
        return Ok(result);
    }

    // ✅ POST: /Task/create
    // Creates a new task
    [HttpPost("create")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.CreateTaskAsync(userId, createTaskDto);
        return Ok(result);
    }

    // ✅ PATCH: /Task/toggle/{taskId}
    // Toggles task completion status (true/false)
    [HttpPatch("toggle/{taskId}")]
    public async Task<IActionResult> ToggleTask(Guid taskId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.ToggleTaskAsync(userId, taskId);
        return Ok(result);
    }

    // ✅ DELETE: /Task/delete/{taskId}
    // Deletes a task
    [HttpDelete("delete/{taskId}")]
    public async Task<IActionResult> DeleteTask(Guid taskId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.DeleteTaskAsync(userId, taskId);
        return Ok(result);
    }

    // ✅ PATCH: /Task/edit/{taskId}
    // Updates a task (partial update supported)
    [HttpPatch("edit/{taskId}")]
    public async Task<IActionResult> EditTask(Guid taskId, EditTaskRequest dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.EditTaskAsync(userId, taskId, dto);
        return Ok(result);
    }

    [HttpGet("{categoryId}")]
    public async Task<IActionResult> GetTaskForCategory(Guid categoryId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Not Allowed");

        var result = await allTasksService.GetTasksForCategoryAsync(userId, categoryId);
        return Ok(result);
    }
}
