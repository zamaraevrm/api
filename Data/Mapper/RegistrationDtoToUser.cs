using Data.Model;
using Data.Request;

namespace Data.Mapper;

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
            Firstname = registrationData.Firstname,
            Surname = registrationData.Surname,
            Patronymic = registrationData.Patronymic,
            HashPassword = password,
            Salt = salt,
            Role = registrationData.Role
        };
        return user;
    }
}