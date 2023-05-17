namespace Data.Model;

public class DocumentTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public ICollection<Document> Documents { get; } = new List<Document>();
    public ICollection<Report> Reports { get; } = new List<Report>();
}