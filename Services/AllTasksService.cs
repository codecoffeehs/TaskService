using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Models;
namespace TaskService.Services;

public class AllTasksService(AppDbContext db)
{
    public async Task<List<TaskItem>> GetAllTasksAsync(Guid userId)
    {
        var tasks = await db.Tasks
            .Where(t => t.CreatedByUserId == userId && !t.IsCompleted)
            .OrderBy(t => t.IsCompleted)   // incomplete first
            .ThenBy(t => t.Due)            // earliest due date first
            .ThenBy(t => t.Repeat)         // non-repeating before repeating
            .ThenBy(t => t.Title)        // stable ordering
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

        return tasks;
    }

    public async Task<List<TaskItem>> GetTasksForCategoryAsync(Guid userId, Guid taskCategoryId)
    {
        var tasks = await db.Tasks.Where(t => t.TaskCategoryId == taskCategoryId && t.CreatedByUserId == userId)
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

        return tasks;
    }

    public async Task<RecentTasksDto> GetRecentTasksAsync(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);


        // ONE query: load all relevant tasks (not completed) for overdue/today/upcoming
        var tasks = await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.CreatedByUserId == userId &&
                !t.IsCompleted
            )
            .OrderBy(t => t.Due)
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

        // split in memory (fast)
        var todayTasks = tasks
            .Where(t => t.Due >= todayStart && t.Due < todayEnd)
            .Take(5)
            .ToList();

        var overdueTasks = tasks
            .Where(t => t.Due < todayStart)
            .Take(5)
            .ToList();

        var upcomingTasks = tasks
            .Where(t => t.Due >= todayEnd)
            .Take(5)
            .ToList();

        // counts
        var todayCount = tasks.Count(t => t.Due >= todayStart && t.Due < todayEnd);
        var overdueCount = tasks.Count(t => t.Due < todayStart);
        var upcomingCount = tasks.Count(t => t.Due >= todayEnd);

        return new RecentTasksDto(
            Today: new TasksSectionDto(todayCount, todayTasks),
            Upcoming: new TasksSectionDto(upcomingCount, upcomingTasks),
            Overdue: new TasksSectionDto(overdueCount, overdueTasks)
        );
    }




    public async Task<TaskItem> CreateTaskAsync(Guid userId, CreateTaskDto dto)
    {
        var newTask = new TaskModel
        {
            CreatedByUserId = userId,
            Title = dto.Title,
            Due = dto.Due,
            Repeat = dto.Repeat,
            TaskCategoryId = dto.TaskCategoryId
        };

        await db.Tasks.AddAsync(newTask);
        await db.SaveChangesAsync();

        // Re-query to include category title
        var task = await db.Tasks
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

        return task;
    }


    public async Task<TaskItem> ToggleTaskAsync(Guid userId, Guid taskId)
    {
        var task = await db.Tasks
            .Where(t => t.CreatedByUserId == userId && t.Id == taskId)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Task Not Found");

        task.IsCompleted = !task.IsCompleted;
        await db.SaveChangesAsync();

        // Re-query with category title
        var result = await db.Tasks
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

        return result;
    }


    public async Task<bool> DeleteTaskAsync(Guid userId, Guid taskId)
    {
        var task = await db.Tasks.Where(t => t.CreatedByUserId == userId && t.Id == taskId).FirstOrDefaultAsync() ??
                   throw new NotFoundException("Task Not Found");
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<TaskItem> EditTaskAsync(Guid userId, Guid taskId, EditTaskRequest request)
    {
        var task = await db.Tasks
            .Where(t => t.CreatedByUserId == userId && t.Id == taskId)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Task Not Found");

        // Update only what frontend sends
        if (request.Title != null)
            task.Title = request.Title;


        if (request.Due.HasValue)
            task.Due = request.Due.Value;

        if (request.IsCompleted.HasValue)
            task.IsCompleted = request.IsCompleted.Value;

        if (request.RepeatType.HasValue)
            task.Repeat = request.RepeatType.Value;


        await db.SaveChangesAsync();

        return new TaskItem(
                    task.Id,
                    task.Title,
                    task.IsCompleted,
                    task.Due,
                    task.Repeat,
                    task.TaskCategoryId,
                    task.TaskCategory.Title,
                    task.TaskCategory.Color,
                    task.TaskCategory.Icon
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
                t.CreatedByUserId == userId &&
                t.Due >= todayStart &&
                t.Due < todayEnd
            )
            .OrderBy(t => t.Due)
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

    public async Task<List<TaskItem>> GetUpcomingTasksAsync(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        var todayEnd = todayStart.AddDays(1);

        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.CreatedByUserId == userId &&
                t.Due >= todayEnd
            )
            .OrderBy(t => t.Due)
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

    public async Task<List<TaskItem>> GetOverdueTasksAsync(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.CreatedByUserId == userId &&
                t.Due < todayStart
            )
            .OrderBy(t => t.Due)
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

}