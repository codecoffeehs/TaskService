namespace Shared.Contracts;

public record TaskUserCreated(
    Guid UserId,
    string FullName,
    string Email
);