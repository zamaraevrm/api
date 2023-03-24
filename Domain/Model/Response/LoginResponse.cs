namespace Domain.Model.Response;

public record LoginResponse(string AccessToken, UserResponseDto User);