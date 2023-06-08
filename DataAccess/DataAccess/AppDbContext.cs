using Data.Model;
using DataAccess.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DataAccess
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions options):base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().UseTptMappingStrategy();
            modelBuilder.ApplyConfiguration(new StudentConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
            //modelBuilder.ApplyConfiguration(new DocumentConfiguration());
        }
        
        public DbSet<User> Users { get; set; }  
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Document> Documents  { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
