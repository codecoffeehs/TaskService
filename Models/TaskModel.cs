using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskService.Models;

[Table("tasks")]
public class TaskModel
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid CreatedByUserId { get; init; }

    [MaxLength(200)]
    public string Title { get; set; } = null!;

    public bool IsCompleted { get; set; } = false;

    public DateTimeOffset? Due { get; set; }

    public RepeatType? Repeat { get; set; } = RepeatType.None;

    public Guid TaskCategoryId { get; set; }
    public TaskCategory TaskCategory { get; set; } = null!;

    public ICollection<TaskCollaborator> TaskCollaborators { get; set; } = [];
}



public enum RepeatType
{
    None = 0,
    Daily = 1,
    EveryOtherDay = 2,
    Weekly = 3,
    Monthly = 4
}
