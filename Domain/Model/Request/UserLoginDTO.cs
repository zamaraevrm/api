namespace Domain.Model.Request;

public record UserLoginDto
    (
        string Login, 
        string Password,
        string Role
    );