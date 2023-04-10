namespace Data.Model;

public class Group
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public int Year { get; set; }

    public List<Course> Courses { get; set; } = null!;
    public List<Student> Students { get; set; } = null!;
}