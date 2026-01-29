using TaskService.Models;

namespace TaskService.Dtos;

public class EditTaskRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTimeOffset? Due { get; set; }
    public RepeatType RepeatType { get; set; } = RepeatType.None;
    public Guid TaskCategoryId { get; set; }
}
