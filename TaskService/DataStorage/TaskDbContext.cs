using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TaskService.DataStorage.Entites;

namespace TaskService.DataStorage
{
    public class TaskDbContext(DbContextOptions<TaskDbContext> options) : DbContext(options)
    {
        public DbSet<TaskItem> Tasks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("TaskService");
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
