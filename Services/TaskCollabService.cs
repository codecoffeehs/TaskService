using System;
using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Models;

namespace TaskService.Services;

public class TaskCollabService(AppDbContext db)
{
    public async Task TaskInviteAsync(Guid userId, Guid taskId, InviteTaskDto dto)
    {
        // 1) Ensure the task exists AND belongs to the inviter (creator)
        var task = await db.Tasks
            .Where(t => t.Id == taskId && t.CreatedByUserId == userId)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Task Not Found");

        // 2) Don’t allow inviting self
        if (userId == dto.InvitedUserId)
            throw new BadRequestException("You cannot invite yourself.");

        // 3) Check if already invited / already member
        var alreadyExists = await db.TaskMembers.AnyAsync(m =>
            m.TaskId == taskId &&
            m.UserId == dto.InvitedUserId &&
            m.Status != TaskMemberStatus.Removed);

        if (alreadyExists)
            throw new BadRequestException("User already invited / already a collaborator for this task.");

        // 4) Create invite
        var invite = new TaskMember
        {
            TaskId = taskId,
            UserId = dto.InvitedUserId,
            Status = TaskMemberStatus.Pending,
            InvitedByUserId = userId,
        };

        db.TaskMembers.Add(invite);

        await db.SaveChangesAsync();

        // Optional: publish event to Notification service / Auth service
        // await eventBus.PublishAsync(new TaskInviteCreated(...));
    }

    public async Task AcceptInviteAsync(Guid userId, Guid taskId)
    {
        var invite = await db.TaskMembers
            .FirstOrDefaultAsync(m => m.TaskId == taskId && m.UserId == userId)
            ?? throw new NotFoundException("Invite not found.");

        if (invite.Status != TaskMemberStatus.Pending)
            throw new BadRequestException("Invite is not pending.");

        invite.Status = TaskMemberStatus.Accepted;

        await db.SaveChangesAsync();
    }
    public async Task RejectInviteAsync(Guid userId, Guid taskId)
    {
        var invite = await db.TaskMembers
            .FirstOrDefaultAsync(m => m.TaskId == taskId && m.UserId == userId)
            ?? throw new NotFoundException("Invite not found.");

        if (invite.Status != TaskMemberStatus.Pending)
            throw new BadRequestException("Invite is not pending.");
        invite.Status = TaskMemberStatus.Rejected;
        await db.SaveChangesAsync();
    }
    public async Task CancelTaskInviteAsync(Guid userId, Guid taskId, InviteTaskDto dto)
    {
        // 1) Ensure the task exists and the current user is the owner/creator
        var taskExists = await db.Tasks
            .AnyAsync(t => t.Id == taskId && t.CreatedByUserId == userId);

        if (!taskExists)
            throw new NotFoundException("Task Not Found");

        // 2) Find invite
        var invite = await db.TaskMembers
            .FirstOrDefaultAsync(m =>
                m.TaskId == taskId &&
                m.UserId == dto.InvitedUserId &&
                m.InvitedByUserId == userId) ?? throw new NotFoundException("Invite Not Found");

        // 3) Only allow cancel if still pending (recommended)
        if (invite.Status != TaskMemberStatus.Pending)
            throw new BadRequestException("Invite cannot be cancelled because it is not pending.");

        // 4) Delete invite row
        db.TaskMembers.Remove(invite);
        await db.SaveChangesAsync();
    }

    public async Task<List<SharedTaskItem>> GetSharedTasksAsync(Guid userId)
    {
        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.CreatedByUserId != userId &&
                t.TaskMembers.Any(m =>
                    m.UserId == userId &&
                    m.Status == TaskMemberStatus.Accepted
                )
            )
            .OrderBy(t => t.Due)
            .ThenBy(t => t.Title)
            .Select(t => new SharedTaskItem(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.Due,
                t.Repeat,
                t.TaskCategoryId,
                t.TaskCategory.Title,
                t.TaskCategory.Color,
                t.TaskCategory.Icon,
                t.CreatedByUserId,
                t.TaskMembers.Count(m => m.Status == TaskMemberStatus.Accepted)
            ))
            .ToListAsync();
    }

    public async Task<List<SharedTaskItem>> GetSharedTaskRequestsAsync(Guid userId)
    {
        return await db.TaskMembers
            .AsNoTracking()
            .Where(m =>
                m.UserId == userId &&
                m.Status == TaskMemberStatus.Pending
            )
            .OrderByDescending(m => m.Id) // better: order by CreatedAt if you add it
            .Select(m => new SharedTaskItem(
               m.Id,
               m.Task.Title,
               m.Task.IsCompleted,
               m.Task.Due,
               m.Task.Repeat,
               m.Task.TaskCategoryId,
               m.Task.TaskCategory.Title,
               m.Task.TaskCategory.Color,
               m.Task.TaskCategory.Icon,
               m.Task.CreatedByUserId,
               null
            ))
            .ToListAsync();
    }

}
