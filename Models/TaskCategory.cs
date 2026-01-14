using System.ComponentModel.DataAnnotations.Schema;

namespace TaskService.Models;

[Table("task_categories")]
public class TaskCategory
{
    public Guid Id {get;init;}

    public string Title {get;set;} = null!;

    public string Color {get;set;} = null!;

    public string Icon {get;set;} = null!;
    public Guid UserId {get;set;}
    public ICollection<TaskModel> Tasks = [];
}