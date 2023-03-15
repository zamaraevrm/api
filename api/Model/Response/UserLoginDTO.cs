namespace api.Model.Response;

public record UserLoginDto
    (
        string Login, 
        string Password
    );