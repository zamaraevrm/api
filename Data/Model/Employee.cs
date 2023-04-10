namespace Data.Model;

public class Employee: User
{

    public List<EmployeeRole> Roles { get; set; } = null!;
    public List<Document> Documents { get; set; } = null!;
    public List<Course> Courses { get; set; } = null!;
}