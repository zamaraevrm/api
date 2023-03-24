namespace Domain.Model
{
    public class User
    {
        public Guid Id { get; set; }
        public string Firstname { get; set; } = default!;
        public string Surname { get; set; } = default!;
        public string? Patronymic { get; set; } 
        public string Email { get; set; } = default!;
        public byte[] HashPassword { get; set; } = default!;
        public byte[] Salt { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
