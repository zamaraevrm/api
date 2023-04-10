namespace Data.Response;

public record UserResponseDto
(
    Guid Id,
    string Firstname, 
    string Surname,  
    string? Patronymic,  
    string Email,  
    string Role  
);
    
