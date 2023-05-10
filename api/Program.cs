using System.Text;
using api;
using api.apis;
using DataAccess.DataAccess;
using Data.JsonConverters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TemplateEngine.Docx;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

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
        policy => policy.RequireAuthenticatedUser().RequireRole("studnt")
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

builder.Services.Configure<JsonOptions>(options => 
    options.SerializerOptions.Converters.Add(new DateOnlyConverter()));

builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddScoped<TokenValidator>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UsePathBase("/api");
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapRoutesAuth();
app.MapRoutesTemplate();
app.MapRoutesGroup();

app.MapGet("/hello-world", (HttpContext context) => context.User)
    .RequireCors("CorsPolicy").RequireAuthorization("user");

app.MapGet("/user", () => "Hello user")
    .RequireCors("CorsPolicy")
    .RequireAuthorization("user");

app.MapRoutesTemplate().RequireCors("CorsPolicy").AllowAnonymous();


app.MapPost("/doc", async (ReportRequest report) =>
{
    string path = @"C:\Users\zamar\Downloads\zachetnaya-vedomost-po-uchebnoj-praktike.docx";
    string name = $"dsjkflk{DateTime.Now.ToShortDateString()}";
    
    //File.Copy(path, name);*/
    
    var content = new Content(
            new FieldContent("Teacher",report.Teacher),
            new FieldContent("Date",report.Date.ToShortDateString()),
            new FieldContent("Group", report.Group),
            new TableContent("Students", report.Students
                .Select(student => 
                    new TableRowContent(
                        new FieldContent("Fullname",student.Fullname),
                        new FieldContent("Assessment",student.Assessment.ToString())
                        )
                )
            ));

    await using var file = File.Open(path, FileMode.Open);
    var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);
    
    
    
    using var doc = new TemplateProcessor(name).SetRemoveContentControls(true);
    doc.FillContent(content);
   
    if (memoryStream.CanSeek) 
         memoryStream.Seek(0, SeekOrigin.Begin);
    return Results.File(memoryStream,
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        name);

    
});

app.Run();

record ReportRequest
(
    string Teacher,
    string Group,
    DateOnly Date,
    List<StudentReportRequest> Students
);

record StudentReportRequest( string Fullname, int Assessment);