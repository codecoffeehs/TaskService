using Microsoft.EntityFrameworkCore;
using TaskService.Context;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Models;

namespace TaskService.Services;

public class TaskCategoryService(AppDbContext db)
{
    // READ (Get all categories)
    public async Task<List<TaskCategoryResponse>> GetCategoriesAsync(Guid userId)
{
    return await db.TaskCategories
        .AsNoTracking()
        .Where(tc => tc.UserId == userId)
        .OrderBy(tc => tc.Title)
        .Select(tc => new TaskCategoryResponse(
            tc.Id,
            tc.Title,
            tc.Color,
            tc.Icon,
            tc.Tasks.Count
        ))
        .ToListAsync();
}


    // CREATE
    public async Task<TaskCategoryResponse> CreateCategoryAsync(Guid userId,CreateTaskCategory dto)
    {

        var exists = await db.TaskCategories
            .AnyAsync(tc => tc.Title.ToLower() == dto.Title.ToLower() && tc.UserId == userId);

        if (exists)
            throw new BadRequestException("Category already exists");

        var newCategory = new TaskCategory
        {
            UserId = userId,
            Title = dto.Title,
            Icon = dto.Icon,
            Color = dto.Color
        };

        await db.TaskCategories.AddAsync(newCategory);
        await db.SaveChangesAsync();

        return new TaskCategoryResponse(newCategory.Id, newCategory.Title,newCategory.Color,newCategory.Icon,newCategory.Tasks.Count);
    }

    // UPDATE
    public async Task<TaskCategoryResponse> UpdateCategoryAsync(Guid categoryId, string title)
    {
        var category = await db.TaskCategories
            .FirstOrDefaultAsync(tc => tc.Id == categoryId)
            ?? throw new NotFoundException("Category not found");

        category.Title = title;
        await db.SaveChangesAsync();

        return new TaskCategoryResponse(category.Id, category.Title,category.Color,category.Icon,category.Tasks.Count);
    }

    // DELETE
    public async Task<bool> DeleteCategoryAsync(Guid categoryId)
    {
        var category = await db.TaskCategories
            .FirstOrDefaultAsync(tc => tc.Id == categoryId)
            ?? throw new NotFoundException("Category not found");

        db.TaskCategories.Remove(category);
        await db.SaveChangesAsync();

        return true;
    }
}
