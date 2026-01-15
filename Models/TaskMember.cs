using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TaskService.Models;

[Table("task_member")]
[Index(nameof(TaskId), nameof(UserId), IsUnique = true)]
public class TaskMember
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid TaskId { get; set; }
    public TaskModel Task { get; set; } = null!;

    public Guid UserId { get; set; }

    public TaskMemberStatus Status { get; set; } = TaskMemberStatus.Pending;

    public Guid InvitedByUserId { get; set; }
}

public enum TaskMemberStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Removed = 3
}
