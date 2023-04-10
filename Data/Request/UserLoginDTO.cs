namespace Data.Request;

public record UserLoginDto
    (
        string Login, 
        string Password,
        string Role
    );