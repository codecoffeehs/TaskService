namespace TaskService.Dtos;

public record TaskItem(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTimeOffset Due
    );