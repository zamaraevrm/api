namespace Data.Model;

public class Employee: User
{

    public ICollection<EmployeeRole> Roles { get; set; } = null!;
    public ICollection<Document> Documents { get; set; } = null!;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Report> Reports { get; } = new List<Report>();
}