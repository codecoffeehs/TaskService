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
                t.TaskCategory.Title
            ))
            .ToListAsync();

        return tasks;
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
                t.TaskCategory.Title
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
                t.TaskCategory.Title
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