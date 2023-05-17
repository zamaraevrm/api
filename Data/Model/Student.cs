namespace Data.Model;

public class Student: User
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public string Number { get; set; } = null!;
    public List<Assessment> Assessments { get; set; } = null!;
    public List<Document> Documents { get; set; } = null!;
}