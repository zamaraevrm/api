using System.Text;
using api;
using DataAccess.DataAccess;
using Domain;
using Domain.Model;
using Domain.Model.Mapper;
using Domain.Model.Request;
using Domain.Model.Response;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey
                (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "user",
        policy => policy.RequireAuthenticatedUser().RequireRole("user")
    );
});

builder.Services.AddCors(options =>
{
        options.AddPolicy("CorsPolicy", policyBuilder =>
        {
            policyBuilder.WithOrigins("https://localhost:5173/")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed(str => true);
        });
});

builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddScoped<TokenValidator>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();



app.MapPost("/api/auth/login", [AllowAnonymous]
async(
    UserLoginDto loginData,
    HttpResponse response,
    TokenGenerator tokens,
    AppDbContext db
) =>
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

    await db.Tokens.AddAsync(new Token (refreshTokenId, user.Id));
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

    return Results.Ok(new LoginResponse(accessToken, user.ToUserResponse()));
}).RequireCors("CorsPolicy");

app.MapPost("/api/auth/signup",  [AllowAnonymous] async(
    HttpRequest request,
    HttpResponse response,
    UserRegistrationDto registrationData,
    AppDbContext context
) =>
{
    if(await context.Users.AnyAsync(user => user.Email == registrationData.Email))
        return Results.Conflict("A user with this email address already exists.");

    var user = registrationData.ToUser();
    
    await context.Users.AddAsync(user);
    await context.SaveChangesAsync();
    
    return Results.Ok();
}).RequireCors("CorsPolicy");

app.MapPost("/api/auth/refresh", [AllowAnonymous] async (
    HttpRequest request,
    HttpResponse response,
    AppDbContext db,
    TokenValidator validator,
    TokenGenerator tokens
) =>
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
    await db.Tokens.AddAsync(new Token (newRefreshTokenId, user.Id));
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
}).RequireCors("CorsPolicy");

app.MapDelete("/api/auth/sign-out", async (
    HttpRequest request,
    HttpResponse response,
    AppDbContext db,
    TokenValidator validator
) =>
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
}).RequireCors("CorsPolicy");



app.MapGet("/api/hello-world", () => "Hello World!")
    .RequireCors("CorsPolicy");

app.MapGet("/api/user", () => "Hello user")
    .RequireCors("CorsPolicy")
    .RequireAuthorization("user");



app.Run();

