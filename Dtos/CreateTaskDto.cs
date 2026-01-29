namespace TaskService.Dtos;

using TaskService.Models;
public record CreateTaskDto(
    string Title,
    string? Description,
    DateTimeOffset? Due,
    RepeatType? Repeat,
    Guid TaskCategoryId
);