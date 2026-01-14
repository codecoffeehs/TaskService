using TaskService.Models;

namespace TaskService.Dtos;

public record EditTaskRequest
(
    string? Title ,
    DateTimeOffset? Due,
    bool? IsCompleted,
    RepeatType? RepeatType
);
