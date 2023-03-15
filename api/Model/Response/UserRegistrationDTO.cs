namespace api.Model.Response;

public record UserRegistrationDto
    (
        string Email, 
        string Name, 
        string Password,
        string Role
    );