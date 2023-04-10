namespace Data.Model;

public class DocumentTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;

    public List<Document> Documents { get; set; } = new();
}