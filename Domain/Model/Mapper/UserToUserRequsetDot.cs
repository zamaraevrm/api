using Domain.Model.Response;

namespace Domain.Model.Mapper;

public static class UserToUserResponseDot
{
    public static UserResponseDto ToUserResponse(this User user)
    {
        return new UserResponseDto(
            user.Id,
            user.Firstname,
            user.Surname,
            user.Patronymic,
            user.Email,
            user.Role
        );
    }
}