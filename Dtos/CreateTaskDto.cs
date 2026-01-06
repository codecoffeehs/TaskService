namespace TaskService.Dtos;

public record CreateTaskDto(
    string Title,
    DateTimeOffset Due
);