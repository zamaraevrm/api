using Data.Mapper;
using DataAccess.DataAccess;
using Microsoft.EntityFrameworkCore;
using NGS.Templater;

namespace api.apis;

public static class TemplateApi
{
    public static RouteGroupBuilder MapTemplate(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("/templates");

        group.MapGet("/{id}",GetTemplate);
        group.MapGet("/", GetTemplates);

        return group;
    }

    private static async Task<IResult> GetTemplates(AppDbContext db)
    {
        var templates = await db.DocumentTemplates
            .Select(template => template.ToTemplateResponse())
            .ToListAsync();

        return Results.Ok(templates);
    }

    private static async Task<IResult> GetTemplate(string id, AppDbContext db)
    {
        var template = await db.DocumentTemplates
            .Where(template => template.Id == Guid.Parse(id))
            .FirstOrDefaultAsync();

        if (template is null) return Results.NotFound();
        
        var factory = Configuration.Factory;
        using var doc = factory.Open(template.Path);

        return Results.Ok(doc.Templater.Tags);
    }
}