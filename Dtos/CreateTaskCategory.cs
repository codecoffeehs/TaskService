namespace TaskService.Dtos;

public record CreateTaskCategory(
    string Title,
    string Color,
    string Icon
);