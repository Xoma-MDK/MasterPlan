using AuthService.DataStorage.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.DataStorage
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
    }

}
