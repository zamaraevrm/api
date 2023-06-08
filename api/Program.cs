using System.Text;
using System.Text.Json.Serialization;
using api;
using api.apis;
using DataAccess.DataAccess;
using Data.JsonConverters;
using Data.Mapper;
using Data.Model;
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
        policy => policy.RequireAuthenticatedUser().RequireRole("student")
    );
});

var origins = builder.Configuration?.GetSection("AllowedOrigins").Get<string[]>() ?? new []{""};
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
{
    options.SerializerOptions.Converters.Add(new DateOnlyConverter()); 
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddScoped<TokenValidator>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UsePathBase("/api");
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapRoutesAuth().RequireCors("CorsPolicy");
app.MapRoutesTemplate();
app.MapRoutesGroup();

app.MapGet("/hello-world", (HttpContext context) => context.User)
    .RequireCors("CorsPolicy").RequireAuthorization("user");

app.MapGet("/user", () => "Hello user")
    .RequireCors("CorsPolicy")
    .RequireAuthorization("user");

app.MapRoutesTemplate().RequireCors("CorsPolicy").AllowAnonymous();

app.MapGet("teacher/{id}/courses", async (string id, AppDbContext db) =>
{
    var courses = await db.Employees
        .Where(t => t.Id == Guid.Parse(id))
        .SelectMany(t => t.Courses.Select(course => course.ToCourseResponse()))
        .ToListAsync();

    return Results.Ok(courses);
});


app.MapPost("/doc/{id}", async (string id, AppDbContext db, ReportRequest report) =>
{
    string templatePath = await db.DocumentTemplates
        .Where(e => e.Id == Guid.Parse(id))
        .Select(e => e.Path)
        .FirstAsync();
    
    const string name = "dsjkflk.docx";
    string outPath = Directory.GetCurrentDirectory() + name;

    var content = new Content(
            new FieldContent("Teacher",report.Teacher),
            new FieldContent("Date",report.Date.ToShortDateString()),
            new FieldContent("Group", report.Group),
            new TableContent("Students", report.Students
                .Select(student => 
                    new TableRowContent(
                        new FieldContent("FullName",student.Fullname),
                        new FieldContent("Number", student.Number),
                        new FieldContent("Assessment",student.Assessment.ToString())
                        )
                )
            ));
    File.Delete(outPath);
    File.Copy(templatePath,outPath);
    

    using(var doc = new TemplateProcessor(outPath)){
        doc.FillContent(content);
        doc.SaveChanges();
    }
    await using var fileStream = File.Open(outPath, FileMode.Open);

    var dataStream = new MemoryStream();
    await fileStream.CopyToAsync(dataStream);
    if (dataStream.CanSeek) 
        dataStream.Seek(0, SeekOrigin.Begin);
    

    
    return Results.File(dataStream,
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

record StudentReportRequest( string Fullname, string Number, int Assessment);