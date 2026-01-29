using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskService.Models;

[Table("task_collaborator")]
public class TaskCollaborator
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public Guid TaskId { get; set; }
    public TaskModel Task { get; set; } = null!;

    public Guid UserId { get; set; }

    public TaskRole TaskRole { get; set; } = TaskRole.Collaborator;
};

public enum TaskRole
{
    Owner = 1,
    Collaborator = 2
}
