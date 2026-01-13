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
            .Where(t => t.UserId == userId && !t.IsCompleted)
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
        var tasks = await db.Tasks.Where(t=>t.TaskCategoryId == taskCategoryId && t.UserId == userId)
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
    var todayStart = now.Date;
    var todayEnd = todayStart.AddDays(1);

    var baseQuery = db.Tasks
        .AsNoTracking()
        .Where(t => t.UserId == userId && !t.IsCompleted);

    static IQueryable<TaskItem> Project(IQueryable<TaskModel> q)
        => q.Select(t => new TaskItem(
            t.Id,
            t.Title,
            t.IsCompleted,
            t.Due,
            t.Repeat,
            t.TaskCategoryId,
            t.TaskCategory.Title,
            t.TaskCategory.Color,
            t.TaskCategory.Icon
        ));

    // TODAY
    var todayQuery = baseQuery.Where(t => t.Due >= todayStart && t.Due < todayEnd);

    var todayCountTask = todayQuery.CountAsync();
    var todayTasksTask = Project(todayQuery
            .OrderBy(t => t.Due)
            .ThenBy(t => t.Repeat)
            .ThenBy(t => t.Title))
        .Take(5)
        .ToListAsync();

    // OVERDUE
    var overdueQuery = baseQuery.Where(t => t.Due < todayStart);

    var overdueCountTask = overdueQuery.CountAsync();
    var overdueTasksTask = Project(overdueQuery
            .OrderBy(t => t.Due)
            .ThenBy(t => t.Repeat)
            .ThenBy(t => t.Title))
        .Take(5)
        .ToListAsync();

    // UPCOMING
    var upcomingQuery = baseQuery.Where(t => t.Due >= todayEnd);

    var upcomingCountTask = upcomingQuery.CountAsync();
    var upcomingTasksTask = Project(upcomingQuery
            .OrderBy(t => t.Due)
            .ThenBy(t => t.Repeat)
            .ThenBy(t => t.Title))
        .Take(5)
        .ToListAsync();

    // run queries in parallel (faster)
    await Task.WhenAll(
        todayCountTask, todayTasksTask,
        overdueCountTask, overdueTasksTask,
        upcomingCountTask, upcomingTasksTask
    );

    return new RecentTasksDto(
        Today: new TasksSectionDto(todayCountTask.Result, todayTasksTask.Result),
        Upcoming: new TasksSectionDto(upcomingCountTask.Result, upcomingTasksTask.Result),
        Overdue: new TasksSectionDto(overdueCountTask.Result, overdueTasksTask.Result)
    );
}



    public async Task<TaskItem> CreateTaskAsync(Guid userId, CreateTaskDto dto)
    {
        var newTask = new TaskModel
        {
            UserId = userId,
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
            .Where(t => t.UserId == userId && t.Id == taskId)
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

    
    public async Task<bool> DeleteTaskAsync(Guid userId,Guid taskId)
    {
        var task = await db.Tasks.Where(t => t.UserId == userId && t.Id == taskId).FirstOrDefaultAsync() ??
                   throw new NotFoundException("Task Not Found");
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return true;
    }
}