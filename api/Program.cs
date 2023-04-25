using System.Text;
using api;
using api.apis;
using DataAccess.DataAccess;
using Data;
using Data.Model;
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


var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
        options.AddPolicy("CorsPolicy", policyBuilder =>
        {
            policyBuilder.WithOrigins(origins)
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

app.UsePathBase("/api");
app.UseRouting();


app.MapAuth();

app.MapGet("/hello-world", () => "Hello World!")
    .RequireCors("CorsPolicy");

app.MapGet("/user", () => "Hello user")
    .RequireCors("CorsPolicy")
    .RequireAuthorization("user");

app.MapTemplate();



app.Run();

