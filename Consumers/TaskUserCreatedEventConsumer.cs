using Shared.Contracts;
using MassTransit;
using TaskService.Dtos;
using TaskService.Services;

namespace TaskService.Consumers;

public class TaskUserCreatedEventConsumer(TaskCategoryService taskCategoryService) : IConsumer<TaskUserCreated>
{
    public async Task Consume(ConsumeContext<TaskUserCreated> context)
    {
        var message = context.Message;
        var dto = new CreateTaskCategory("Others", "08080", "o.circle.fill");
        await taskCategoryService.CreateCategoryAsync(message.UserId, dto);
    }
}