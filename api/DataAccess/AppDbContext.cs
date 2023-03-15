using api.Model;
using Microsoft.EntityFrameworkCore;

namespace api.DataAccess
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions options):base(options) { }

        public DbSet<User> Users { get; set; }  
        public DbSet<Token> Tokens { get; set; }
    }
}
