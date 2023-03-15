using api.Model.Response;
using Microsoft.AspNetCore.Identity;

namespace api.Model.Mapper;

public static class RegistrationDtoToUser
{
    public static User ToUser(this UserRegistrationDto registrationData)
    {
        PasswordHasher.HashPassword(registrationData.Password, 
            out byte[] password, 
            out byte[] salt);
            
        User user = new()
        {
            Email = registrationData.Email,
            Name = registrationData.Name,
            HashPassword = password,
            Salt = salt,
            Role = registrationData.Role
        };
        return user;
    }
}