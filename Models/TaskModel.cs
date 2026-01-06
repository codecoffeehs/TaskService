using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskService.Models;

[Table("tasks")]
public class TaskModel
{
    public Guid Id { get; init; }
    
    public Guid UserId { get; init; }
    
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    public bool IsCompleted { get; set; } = false;
    
    public DateTimeOffset Due { get; set; }
}