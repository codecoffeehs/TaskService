namespace TaskService.Dtos;

using TaskService.Models;

public record TaskItem(
    Guid Id,
    string Title,
    string Description,
    bool IsCompleted,
    DateTimeOffset? Due,
    RepeatType? RepeatType,
    Guid CategoryId,
    string CategoryTitle,
    string Color,
    string? Icon,
    bool IsCreator
);
