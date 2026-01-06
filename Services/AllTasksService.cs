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
        var tasks = await db.Tasks.Where(t => t.UserId == userId).Select(t=>new TaskItem(t.Id,t.Title,t.IsCompleted,t.Due)).ToListAsync();
        return tasks;
    }

    public async Task<TaskItem> CreateTaskAsync(Guid userId,CreateTaskDto dto)
    {
        var newTask = new TaskModel
        {
            UserId = userId,
            Title = dto.Title,
            Due = dto.Due
        };
        await db.Tasks.AddAsync(newTask);
        await db.SaveChangesAsync();
        return new TaskItem(newTask.Id,newTask.Title,newTask.IsCompleted,newTask.Due);
    }

    public async Task<TaskItem> ToggleTaskAsync(Guid userId,Guid taskId)
    {
        var task = await db.Tasks.Where(t => t.UserId == userId && t.Id == taskId).FirstOrDefaultAsync() ??
                   throw new NotFoundException("Task Not Found");
        task.IsCompleted = !task.IsCompleted;
        await db.SaveChangesAsync();
        return new TaskItem(task.Id,task.Title,task.IsCompleted,task.Due);
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