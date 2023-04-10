namespace Data.Model;

public class Document
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid TemplateId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid StudentId { get; set; }

    public DocumentTemplate Template { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}