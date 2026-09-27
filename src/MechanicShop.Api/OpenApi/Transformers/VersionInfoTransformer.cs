using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MechanicShop.Api.OpenApi.Transformers;

internal sealed class VersionInfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document, 
        OpenApiDocumentTransformerContext context, 
        CancellationToken ct)
    {
        var version = context.DocumentName;

        document.Info.Version = version;
        document.Info.Title = $"MechanicShop API {version}";

        return Task.CompletedTask;
    }
}