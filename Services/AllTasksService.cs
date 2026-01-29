using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Models;

namespace TaskService.Services;

public class AllTasksService(AppDbContext db)
{

    public async Task<TaskItem> CreateTaskAsync(Guid userId, CreateTaskDto dto)
    {
        var newTask = new TaskModel
        {
            CreatedByUserId = userId,
            Title = dto.Title,
            TaskCategoryId = dto.TaskCategoryId,
            Due = dto.Due,
            Repeat = dto.Repeat
        };

        db.Tasks.Add(newTask);

        // 🔑 REQUIRED: creator must be collaborator (Owner)
        db.TaskCollaborators.Add(new TaskCollaborator
        {
            TaskId = newTask.Id,
            UserId = userId,
            TaskRole = TaskRole.Owner
        });

        await db.SaveChangesAsync();

        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == newTask.Id)
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .FirstAsync();
    }


    // =====================================================
    // GET ALL TASKS
    // CHANGE: CreatedByUserId -> TaskCollaborators
    // =====================================================
    public async Task<List<TaskItem>> GetAllTasksAsync(Guid userId)
    {
        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                !t.IsCompleted &&
                db.TaskCollaborators.Any(c =>
                    c.TaskId == t.Id &&
                    c.UserId == userId))
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.Due)
            .ThenBy(t => t.Repeat)
            .ThenBy(t => t.Title)
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .ToListAsync();
    }

    // =====================================================
    // GET TASKS FOR CATEGORY
    // =====================================================
    public async Task<List<TaskItem>> GetTasksForCategoryAsync(Guid userId, Guid taskCategoryId)
    {
        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.TaskCategoryId == taskCategoryId &&
                db.TaskCollaborators.Any(c =>
                    c.TaskId == t.Id &&
                    c.UserId == userId))
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .ToListAsync();
    }

    // =====================================================
    // TOGGLE TASK
    // CHANGE: creator-only -> collaborator
    // =====================================================
    public async Task<TaskItem> ToggleTaskAsync(Guid userId, Guid taskId)
    {
        var task = await db.Tasks
            .FirstOrDefaultAsync(t =>
                t.Id == taskId &&
                db.TaskCollaborators.Any(c =>
                    c.TaskId == t.Id &&
                    c.UserId == userId))
            ?? throw new NotFoundException("Task Not Found");

        task.IsCompleted = !task.IsCompleted;
        await db.SaveChangesAsync();

        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .FirstAsync();
    }

    // =====================================================
    // DELETE TASK
    // CHANGE: only OWNER can delete
    // =====================================================
    public async Task<bool> DeleteTaskAsync(Guid userId, Guid taskId)
    {
        var isOwner = await db.TaskCollaborators.AnyAsync(c =>
            c.TaskId == taskId &&
            c.UserId == userId &&
            c.TaskRole == TaskRole.Owner);

        if (!isOwner)
            throw new ForbiddenException("Only owner can delete task");

        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new NotFoundException("Task Not Found");

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return true;
    }

    // =====================================================
    // EDIT TASK
    // CHANGE: creator-only -> collaborator
    // =====================================================
    public async Task<TaskItem> EditTaskAsync(Guid userId, Guid taskId, EditTaskRequest request)
    {
        var hasAccess = await db.TaskCollaborators.AnyAsync(c =>
            c.TaskId == taskId &&
            c.UserId == userId);

        if (!hasAccess)
            throw new ForbiddenException("No access to task");

        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new NotFoundException("Task Not Found");

        task.Title = request.Title;
        task.IsCompleted = request.IsCompleted;
        task.Due = request.Due;
        task.Repeat = request.RepeatType;
        task.TaskCategoryId = request.TaskCategoryId;

        await db.SaveChangesAsync();

        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .FirstAsync();
    }

    public async Task<RecentTasksDto> GetRecentTasksAsync(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;

        // Today range in UTC
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);

        // Load all active tasks once
        var tasks = await db.Tasks
            .AsNoTracking()
            .Where(t => t.CreatedByUserId == userId && !t.IsCompleted)
            .OrderBy(t => t.Due == null)          // ✅ Due tasks first, No-due tasks later
            .ThenBy(t => t.Due)                   // ✅ sorts due tasks by date
            .ThenBy(t => t.Title)
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,                            // ✅ DateTimeOffset?
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .ToListAsync();

        // Split once
        var dueTasks = tasks.Where(t => t.Due.HasValue).ToList();
        var noDueTasks = tasks.Where(t => !t.Due.HasValue).ToList();

        // Sections
        var todayTasks = dueTasks
            .Where(t => t.Due!.Value >= todayStart && t.Due!.Value < todayEnd)
            .Take(5)
            .ToList();

        var overdueTasks = dueTasks
            .Where(t => t.Due!.Value < todayStart)  // ✅ no-due cannot reach here
            .Take(5)
            .ToList();

        var upcomingTasks = dueTasks
            .Where(t => t.Due!.Value >= todayEnd)
            .Take(5)
            .ToList();

        var noDueTop = noDueTasks
            .Take(5)
            .ToList();

        // Counts
        var todayCount = dueTasks.Count(t => t.Due!.Value >= todayStart && t.Due!.Value < todayEnd);
        var overdueCount = dueTasks.Count(t => t.Due!.Value < todayStart);
        var upcomingCount = dueTasks.Count(t => t.Due!.Value >= todayEnd);
        var noDueCount = noDueTasks.Count;

        return new RecentTasksDto(
            Today: new TasksSectionDto(todayCount, todayTasks),
            Upcoming: new TasksSectionDto(upcomingCount, upcomingTasks),
            Overdue: new TasksSectionDto(overdueCount, overdueTasks),
            NoDue: new TasksSectionDto(noDueCount, noDueTop)   // ✅ separate section
        );
    }


    public async Task<List<TaskItem>> GetTodayTasksAsync(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);

        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.Due >= todayStart &&
                t.Due < todayEnd &&
                db.TaskCollaborators.Any(c =>
                    c.TaskId == t.Id &&
                    c.UserId == userId))
            .OrderBy(t => t.Due)
            .ThenBy(t => t.Title)
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetUpcomingTasksAsync(Guid userId)
    {
        var todayStart = DateTimeOffset.UtcNow.Date.AddDays(1);

        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.Due >= todayStart &&
                db.TaskCollaborators.Any(c =>
                    c.TaskId == t.Id &&
                    c.UserId == userId))
            .OrderBy(t => t.Due)
            .ThenBy(t => t.Title)
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .ToListAsync();
    }
    public async Task<List<TaskItem>> GetOverdueTasksAsync(Guid userId)
    {
        var todayStart = DateTimeOffset.UtcNow.Date;

        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.Due < todayStart &&
                db.TaskCollaborators.Any(c =>
                    c.TaskId == t.Id &&
                    c.UserId == userId))
            .OrderBy(t => t.Due)
            .ThenBy(t => t.Title)
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .ToListAsync();
    }
    public async Task<List<TaskItem>> GetNodueTasksAsync(Guid userId)
    {
        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.Due == null &&
                db.TaskCollaborators.Any(c =>
                    c.TaskId == t.Id &&
                    c.UserId == userId))
            .OrderBy(t => t.Title)
            .Select(t => new TaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon
            ))
            .ToListAsync();
    }



}
