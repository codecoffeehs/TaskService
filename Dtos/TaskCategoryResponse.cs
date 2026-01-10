namespace TaskService.Dtos;

public record TaskCategoryResponse(
    Guid Id,
    string Title,
    string Color,
    string Icon
);