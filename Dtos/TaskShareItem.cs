namespace TaskService.Dtos;

public record TaskShareItem(
    Guid Id,
    string Title,
    string Description,
    string InvitedByUserEmail,
    DateTimeOffset SharedOn
);
