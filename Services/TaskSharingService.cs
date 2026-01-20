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
    //     public async Task AcceptInviteAsync(Guid userId, Guid inviteId)
    // {
    //     var invite = await db.TaskInvites
    //         .Include(i => i.Task)
    //         .FirstOrDefaultAsync(i => i.Id == inviteId && i.SharedWithUserId == loggedInUserId)
    //         ?? throw new NotFoundException("Invite not found");

    //     if (invite.TaskInviteStatus != TaskInviteStatus.Pending)
    //         throw new BadRequestException("Invite already processed");

    //     // Clone / copy task into invited user's task list
    //     var copiedTask = new TaskModel
    //     {
    //         Id = Guid.NewGuid(),
    //         CreatedByUserId = loggedInUserId,
    //         Title = invite.Task.Title,
    //         Due = invite.Task.Due,
    //         Repeat = invite.Task.Repeat,
    //         IsCompleted = false,

    //         // If your categories are shared globally, you can reuse same CategoryId:
    //         TaskCategoryId = invite.Task.TaskCategoryId
    //     };

    //     db.Tasks.Add(copiedTask);

    //     // Mark invite accepted (do not delete)
    //     invite.TaskInviteStatus = TaskInviteStatus.Accepted;
    //     invite.CopiedTaskId = copiedTask.Id;

    //     await db.SaveChangesAsync();
    // }


}
