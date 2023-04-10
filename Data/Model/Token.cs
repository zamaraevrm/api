namespace Data.Model;

public class Token
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } 
    public User User { get; set; } = null!;
};
