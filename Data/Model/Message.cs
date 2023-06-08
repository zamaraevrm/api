namespace Data.Model;

public class Message
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = System.DateTime.Now;
    public Guid AddresseeId { get; set; }
    public Guid SenderId { get; set; }

    public User Addressee { get; set; } = null!;
    public User Sender { get; set; } = null!;
}