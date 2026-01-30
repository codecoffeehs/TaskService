using System;
using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Models;

namespace TaskService.Services;

public class TaskSharingService(AppDbContext db)
{
    // =========================================================
    // 1️⃣ SEND INVITE
    // CHANGE:
    // ❌ Earlier: only task creator could invite
    // ✅ Now: task OWNER (collaborator) can invite
    // =========================================================
    public async Task SendTaskInviteAsync(
        Guid userId,
        Guid taskId,
        string sharedByEmail,
        ShareTaskDto dto)
    {
        // CHANGED:
        // We now validate using TaskCollaborators instead of CreatedByUserId
        var isOwner = await db.TaskCollaborators.AnyAsync(c =>
            c.TaskId == taskId &&
            c.UserId == userId &&
            c.TaskRole == TaskRole.Owner);

        if (!isOwner)
            throw new ForbiddenException("Only task owners can invite users");

        // SAME:
        // Prevent duplicate pending invites
        var alreadyInvited = await db.TaskInvites.AnyAsync(i =>
            i.TaskId == taskId &&
            i.SharedWithUserId == dto.SharedWithUserId &&
            i.TaskInviteStatus == TaskInviteStatus.Pending);

        if (alreadyInvited)
            throw new BadRequestException("Invite already pending");

        var newTaskInvite = new TaskInvite
        {
            TaskId = taskId,
            InvitedByUserId = userId,
            InvitedByUserEmail = sharedByEmail,
            SharedWithUserId = dto.SharedWithUserId,
            TaskInviteStatus = TaskInviteStatus.Pending,
            SharedOn = DateTimeOffset.UtcNow
        };

        db.TaskInvites.Add(newTaskInvite);
        await db.SaveChangesAsync();
    }

    // =========================================================
    // 2️⃣ GET INVITES FOR USER
    // NO CHANGE:
    // Invites are still temporary requests
    // =========================================================
    public async Task<List<TaskShareItem>> GetSharedTaskRequestsAsync(Guid userId)
    {
        return await db.TaskInvites
            .Include(i => i.Task)
            .Where(i =>
                i.SharedWithUserId == userId &&
                i.TaskInviteStatus == TaskInviteStatus.Pending)
            .Select(i => new TaskShareItem(
                i.Id,
                i.Task.Title,
                i.Task.Description,
                i.InvitedByUserEmail,
                i.SharedOn))
            .ToListAsync();
    }

    // =========================================================
    // 3️⃣ ACCEPT INVITE
    // CHANGE:
    // ❌ Earlier: created a NEW task (wrong)
    // ✅ Now: add user as TaskCollaborator (correct)
    // =========================================================
    public async Task AcceptInviteAsync(
        Guid userId,
        Guid inviteId)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var invite = await db.TaskInvites
            .Include(i => i.Task)
                .ThenInclude(t => t.TaskCategory)
            .FirstOrDefaultAsync(i =>
                i.Id == inviteId &&
                i.SharedWithUserId == userId)
            ?? throw new NotFoundException("Invite not found");

        if (invite.TaskInviteStatus != TaskInviteStatus.Pending)
            throw new BadRequestException("Invite already processed");

        // NEW:
        // Ensure user is not already a collaborator
        var alreadyCollaborator = await db.TaskCollaborators.AnyAsync(c =>
            c.TaskId == invite.TaskId &&
            c.UserId == userId);

        if (alreadyCollaborator)
            throw new BadRequestException("User already has access to this task");

        // NEW:
        // Grant access by adding collaborator
        db.TaskCollaborators.Add(new TaskCollaborator
        {
            TaskId = invite.TaskId,
            UserId = userId,
            TaskRole = TaskRole.Collaborator
        });

        // SAME:
        invite.TaskInviteStatus = TaskInviteStatus.Accepted;

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        // CHANGED:
        // // Return the EXISTING shared task (not a copy)
        // return new TaskItem(
        //     invite.Task.Id,
        //     invite.Task.Title,
        //     invite.Task.Description,
        //     invite.Task.IsCompleted,
        //     invite.Task.Due,
        //     invite.Task.Repeat,
        //     invite.Task.TaskCategoryId,
        //     invite.Task.TaskCategory.Title,
        //     invite.Task.TaskCategory.Color,
        //     invite.Task.TaskCategory.Icon,
        //     inv
        // );
    }

    // =========================================================
    // 4️⃣ REJECT INVITE
    // NO CHANGE:
    // Simple status update
    // =========================================================
    public async Task RejectInvite(Guid userId, Guid inviteId)
    {
        var invite = await db.TaskInvites
            .FirstOrDefaultAsync(i =>
                i.Id == inviteId &&
                i.SharedWithUserId == userId)
            ?? throw new NotFoundException("Invite not found");

        if (invite.TaskInviteStatus != TaskInviteStatus.Pending)
            throw new BadRequestException("Invite already processed");

        invite.TaskInviteStatus = TaskInviteStatus.Rejected;
        await db.SaveChangesAsync();
    }
}
