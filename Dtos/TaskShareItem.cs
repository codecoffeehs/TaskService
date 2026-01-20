namespace TaskService.Dtos;

public record TaskShareItem(
    Guid Id,
    string Title,
    string InvitedByUserEmail,
    DateTimeOffset SharedOn
);
