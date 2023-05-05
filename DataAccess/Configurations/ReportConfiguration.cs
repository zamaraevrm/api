using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class ReportConfiguration: IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasOne<DocumentTemplate>(e => e.Template)
            .WithMany(e => e.Reports)
            .HasForeignKey(e => e.TemplateId)
            .HasPrincipalKey(e => e.Id);

        builder.HasOne<Employee>(e => e.Teacher)
            .WithMany(e => e.Reports)
            .HasForeignKey(e => e.TeacherId)
            .HasPrincipalKey(e => e.Id);
    }
}