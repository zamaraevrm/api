namespace Data.Model;

public class Assessment
{
    public Guid Id { get; set; }
    public int Score { get; set; }
    public DateTime Date { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }

    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}