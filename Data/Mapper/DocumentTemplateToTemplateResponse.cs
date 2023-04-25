using Data.Model;
using Data.Response;

namespace Data.Mapper;

public static class DocumentTemplateToTemplateResponse
{
    public static TemplateResponse ToTemplateResponse(this DocumentTemplate template) =>
        new TemplateResponse()
        {
            Id = template.Id,
            Name = template.Name
        };
}