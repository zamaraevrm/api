using Data.Model;
using DataAccess.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace api.apis;

public static class GroupApi
{
    public static RouteGroupBuilder MapRoutesGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/groups");

        group.MapGet("/", GetGroups);
        group.MapGet("/{id}", GetGroupById);
        group.MapGet("/{id}/students", GetStudents);
        
        return group;
    }

    private static async Task<IResult> GetStudents(string id, AppDbContext db)
    {
        var students = await db.Groups
            .Where(group => group.Id == Guid.Parse(id))
            .SelectMany(e => e.Students)
            .ToListAsync();

        return Results.Ok(students);
    }

    private static async Task<IResult> GetGroupById(string id, AppDbContext db)
    {
        var grope = await db.Groups
            .Where(group => group.Id == Guid.Parse(id))
            .Include(group => group.Students)
            .Include(group => group.Courses)
            .FirstOrDefaultAsync();

        return Results.Ok(grope);
    }

    private static async Task<List<Response>> GetGroups(HttpContext context, AppDbContext db)
    {
        return await db.Groups.Select(group => group.GroupToResponse()).ToListAsync();
    }
}

record Response(Guid Id, string Name, int Year);

file static class Mapper
{
    public static Response GroupToResponse(this Group group)
    {
        return new Response(group.Id, group.Name, group.Year);

    }
}
