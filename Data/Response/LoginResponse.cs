namespace Data.Response;

public record LoginResponse(string AccessToken, UserResponseDto User);