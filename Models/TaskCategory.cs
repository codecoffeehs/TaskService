using System.ComponentModel.DataAnnotations.Schema;

namespace TaskService.Models;

[Table("task_categories")]
public class TaskCategory
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get; set; } = null!;

    public string Color { get; set; } = null!;

    public string? Icon { get; set; }
    public Guid UserId { get; set; }
    public ICollection<TaskModel> Tasks { get; set; } = [];
}