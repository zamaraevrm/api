using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace DataAccess.Configurations;

public class EmployeeConfiguration: IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasMany(employee => employee.Documents)
            .WithOne(document => document.Employee)
            .OnDelete(DeleteBehavior.NoAction);
    }
}