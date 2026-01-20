using System.ComponentModel.DataAnnotations.Schema;

namespace TaskService.Models;

[Table("task_share_invites")]
public class TaskInvite
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid TaskId { get; set; }
    public TaskModel Task { get; set; } = null!;

    public Guid InvitedByUserId { get; set; }
    public string InvitedByUserEmail { get; set; } = null!;

    public Guid SharedWithUserId { get; set; }
    public TaskInviteStatus TaskInviteStatus { get; set; } = TaskInviteStatus.Pending;

    public DateTimeOffset SharedOn { get; set; } = DateTimeOffset.UtcNow;

}

public enum TaskInviteStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}
