using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DataAccess
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions options):base(options) { }

        public DbSet<User> Users { get; set; }  
        public DbSet<Token> Tokens { get; set; }
    }
}
