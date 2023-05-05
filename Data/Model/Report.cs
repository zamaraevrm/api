namespace Data.Model;

public class Report
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid GroupId { get; set; }
    public Guid TeacherId { get; set; }
    public string Path { get; set; } = "";
    public DateTime Date { get; set; }

    public DocumentTemplate Template { get; set; } = null!;
    public Group Group { get; set; } = null!;
    public Employee Teacher { get; set; } = null!;
}