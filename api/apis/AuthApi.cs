using DataAccess.DataAccess;
using Data;
using Data.Mapper;
using Data.Model;
using Data.Request;
using Data.Response;
using Microsoft.EntityFrameworkCore;

namespace api.apis;

public static class AuthApi
{
    public static RouteGroupBuilder MapRoutesAuth(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth");
        
        group.MapPost("/login", Login).AllowAnonymous();
        group.MapPost("/signup", SignUp).AllowAnonymous();
        group.MapPut("/refresh", Refresh).AllowAnonymous();
        group.MapDelete("/sign-out", Logout).AllowAnonymous();
        group.MapPost("/signup-students", SignupStudents);
        
        return group;
    }

    private static async Task<IResult> SignupStudents(List<Student> students, AppDbContext db)
    {
        if (students.Count == 0)
            return Results.BadRequest("empty array");

        await db.Students.AddRangeAsync(students);
        await db.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> Logout(
        HttpRequest request,
        HttpResponse response,
        AppDbContext db,
        TokenValidator validator
    )
    {
        var refreshToken = request.Cookies["refresh_token"];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Results.BadRequest("Please include a refresh token in the request.");

        var tokenIsValid = validator.TryValidateRefreshToken(refreshToken, out var tokenId);
        if (!tokenIsValid) return Results.BadRequest("Invalid refresh token.");

        var token = await db.Tokens.Where(token => token.Id == tokenId).FirstOrDefaultAsync();
        if (token is null) return Results.BadRequest("Refresh token not found.");

        db.Tokens.Remove(token);
        await db.SaveChangesAsync();

        response.Cookies.Delete("refresh_token");
        return Results.NoContent();
    }
    
    private static async Task<IResult> Refresh(
        HttpRequest request,
        HttpResponse response,
        AppDbContext db,
        TokenValidator validator,
        TokenGenerator tokens
    ) 
    {
        var refreshToken = request.Cookies["refresh_token"];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Results.BadRequest("Please include a refresh token in the request.");

        var tokenIsValid = validator.TryValidateRefreshToken(refreshToken, out var tokenId);
        if (!tokenIsValid) return Results.BadRequest("Invalid refresh token.");

        var token = await db.Tokens.Where(token => token.Id == tokenId).FirstOrDefaultAsync();
        if (token is null) return Results.BadRequest("Refresh token not found.");

        var user = await db.Users.Where(u => u.Id == token.UserId).FirstOrDefaultAsync();
        if (user is null) return Results.BadRequest("User not found.");

        var accessToken = tokens.GenerateAccessToken(user);
        var (newRefreshTokenId, newRefreshToken) = tokens.GenerateRefreshToken();

        db.Tokens.Remove(token);
        
        await db.Tokens.AddAsync(new Token ()
        {
            Id = newRefreshTokenId, 
            UserId = user.Id
        });
        await db.SaveChangesAsync();

        response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(1),
            HttpOnly = true,
            IsEssential = true,
            MaxAge = new TimeSpan(1, 0, 0, 0),
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Results.Ok(accessToken);
    }
    
    
    private static async  Task<IResult> SignUp(
        HttpRequest request,
        HttpResponse response,
        UserRegistrationDto registrationData,
        AppDbContext context
    ) 
    {
        if(await context.Users.AnyAsync(user => user.Email == registrationData.Email))
            return Results.Conflict("A user with this email address already exists.");

        var user = registrationData.ToUser();

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
            
        return Results.Ok();
    }
    
    private static async Task<IResult> Login(
        UserLoginDto loginData,
        HttpResponse response,
        TokenGenerator tokens,
        AppDbContext db
    )
    {
        var user = await db.Users.Where(u => u.Email == loginData.Login).FirstOrDefaultAsync();
        if (user is null)
            return Results.NotFound("User with this email address not found");

        if (!PasswordHasher.VerifyPassword(loginData.Password, user.HashPassword, user.Salt))
            return Results.BadRequest("Incorrect password");

        if (!string.Equals(user.Role, loginData.Role, StringComparison.CurrentCultureIgnoreCase))
            return Results.BadRequest("there is no user with this role");
            
        var accessToken = tokens.GenerateAccessToken(user);
        var (refreshTokenId, refreshToken) = tokens.GenerateRefreshToken();

        await db.Tokens.AddAsync(new Token()
        {
            Id = refreshTokenId, 
            UserId = user.Id
        });
        await db.SaveChangesAsync();

        response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(1),
            HttpOnly = true,
            IsEssential = true,
            MaxAge = new TimeSpan(1, 0, 0, 0),
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Results.Ok(accessToken);
    }
}