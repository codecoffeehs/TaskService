namespace TaskService.Dtos;
using TaskService.Models;
public record CreateTaskDto(
    string Title,
    DateTimeOffset Due,
    RepeatType Repeat,
    Guid TaskCategoryId
);