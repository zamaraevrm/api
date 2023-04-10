namespace Data.Request;

public record UserRegistrationDto
    (
        string Email, 
        string Firstname,
        string Surname,
        string? Patronymic,
        string Password,
        string Role
    );