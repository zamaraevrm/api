namespace Data.Model;

public class Course
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public Guid TeacherId { get; set; }
    public float Duration { get; set; }
    public string Type { get; set; } = null!;

    public Lesson Lesson { get; set; } = null!;
    public Employee Teacher { get; set; } = null!;

    public List<Assessment> Assessments { get; set; } = null!;
}