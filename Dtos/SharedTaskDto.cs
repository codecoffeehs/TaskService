namespace TaskService.Dtos;

using TaskService.Models;

public record SharedTaskItem(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTimeOffset Due,
    RepeatType RepeatType,
    Guid CategoryId,
    string CategoryTitle,
    string Color,
    string Icon,

    Guid SharedByUserId,
    int? MembersCount
);
