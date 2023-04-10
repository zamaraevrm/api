using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class StudentConfiguration: IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasMany(student => student.Documents)
            .WithOne(document => document.Student)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(student => student.Assessments)
            .WithOne(assessment => assessment.Student)
            .OnDelete(DeleteBehavior.NoAction);
    }
}