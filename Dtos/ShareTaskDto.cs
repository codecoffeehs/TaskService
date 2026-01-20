namespace TaskService.Dtos;

public record ShareTaskDto(
    string SharedByEmail,
    Guid SharedWithUserId
);
