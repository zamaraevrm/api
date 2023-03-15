namespace api.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;    
        public string Email { get; set; } = default!;
        public byte[] HashPassword { get; set; } = default!;
        public byte[] Salt { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
