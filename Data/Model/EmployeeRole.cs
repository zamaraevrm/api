namespace Data.Model;

public class EmployeeRole
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;

    public List<Employee> Employees { get; } = new();
}