using Microsoft.EntityFrameworkCore;
using TaskService.Models;


namespace TaskService.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskModel> Tasks { get; set; }
    public DbSet<TaskCategory> TaskCategories { get; set; }
    public DbSet<TaskMember> TaskMembers { get; set; }
}