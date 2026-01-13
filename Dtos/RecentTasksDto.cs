namespace TaskService.Dtos;

public record TasksSectionDto(
    int Count,
    List<TaskItem> Tasks
);

public record RecentTasksDto(
    TasksSectionDto Today,
    TasksSectionDto Upcoming,
    TasksSectionDto Overdue
);
