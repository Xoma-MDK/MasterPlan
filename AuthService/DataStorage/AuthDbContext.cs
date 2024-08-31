using AuthService.DataStorage.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AuthService.DataStorage
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("AuthService");
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }

}
