using System;
using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Models;
namespace TaskService.Services;

public class TaskSharingService(AppDbContext db)
{
    public async Task SendTaskInviteAsync(Guid userId, Guid taskId, string sharedByEmail, ShareTaskDto dto)
    {
        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedByUserId == userId)
            ?? throw new NotFoundException("Task Not Found");

        var alreadyInvited = await db.TaskInvites.AnyAsync(i =>
            i.TaskId == taskId &&
            i.SharedWithUserId == dto.SharedWithUserId &&
            i.TaskInviteStatus == TaskInviteStatus.Pending);

        if (alreadyInvited)
            throw new BadRequestException("Invite already pending");

        var newTaskInvite = new TaskInvite
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            InvitedByUserId = userId,
            InvitedByUserEmail = sharedByEmail,
            SharedWithUserId = dto.SharedWithUserId,
            TaskInviteStatus = TaskInviteStatus.Pending,
            SharedOn = DateTimeOffset.UtcNow
        };

        db.TaskInvites.Add(newTaskInvite);
        await db.SaveChangesAsync();
    }
    public async Task<List<TaskShareItem>> GetSharedTaskRequestsAsync(Guid userId)
    {
        var sharedtasksRequests = await db.TaskInvites.Where(t => t.SharedWithUserId == userId && t.TaskInviteStatus == TaskInviteStatus.Pending).Select(t => new TaskShareItem(t.Id, t.Task.Title, t.InvitedByUserEmail, t.SharedOn)).ToListAsync();
        return sharedtasksRequests;

    }
    public async Task<TaskItem> AcceptInviteAndCreateTask(
    Guid userId,
    Guid inviteId,
    CreateTaskDto dto)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var invite = await db.TaskInvites
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.SharedWithUserId == userId)
            ?? throw new NotFoundException("Invite not found");

        if (invite.TaskInviteStatus != TaskInviteStatus.Pending)
            throw new BadRequestException("Invite already processed");

        var newTask = new TaskModel
        {
            CreatedByUserId = userId,
            Title = dto.Title,
            TaskCategoryId = dto.TaskCategoryId,
            Due = dto.Due,
            Repeat = dto.Repeat
        };

        invite.TaskInviteStatus = TaskInviteStatus.Accepted;

        db.Tasks.Add(newTask);
        await db.SaveChangesAsync();

        await tx.CommitAsync();

        // Single clean projection after save
        var task = await db.Tasks
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

        return task;
    }

    public async Task RejectInvite(
    Guid userId,
    Guid inviteId)
    {

        var invite = await db.TaskInvites
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.SharedWithUserId == userId)
            ?? throw new NotFoundException("Invite not found");

        if (invite.TaskInviteStatus != TaskInviteStatus.Pending)
            throw new BadRequestException("Invite already processed");

        invite.TaskInviteStatus = TaskInviteStatus.Rejected;

        await db.SaveChangesAsync();

    }

}
