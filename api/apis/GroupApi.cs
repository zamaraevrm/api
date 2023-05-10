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
        
        return group;
    }

    private static async Task GetGroupById(string id, AppDbContext db)
    {
        var students = await db.Groups
            .Where(group => group.Id == Guid.Parse(id))
            .SelectMany(e => e.Students)
            .ToListAsync();
            
        var grope = await db.Groups
            .Where(group => group.Id == Guid.Parse(id))
            .FirstAsync();
    }

    private static async Task<List<response>> GetGroups(HttpContext context, AppDbContext db)
    {
        return await db.Groups.Select(group => group.GroupToResponse()).ToListAsync();
    }
}

record response(Guid Id, string Name, int Year);

file static class maper
{
    public static response GroupToResponse(this Group group)
    {
        return new response(group.Id, group.Name, group.Year);

    }
}
