using Microsoft.OpenApi;
using Projet.Application.Common;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Projet.API.Swagger;

/// <summary>400 (si l'action reçoit quelque chose) et 500 partout, par filtre : un attribut sur le contrôleur de base effacerait les 200.</summary>
public sealed class CommonResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses ??= [];

        var schema = context.SchemaGenerator.GenerateSchema(typeof(ApiResponse<object>), context.SchemaRepository);

        if (context.ApiDescription.ParameterDescriptions.Count > 0)
            Add(operation, "400", "Requête invalide : format, champ obligatoire ou règle de validation.", schema);

        Add(operation, "500", "Erreur inattendue. Citer le traceId pour le diagnostic.", schema);
    }

    private static void Add(OpenApiOperation operation, string status, string description, IOpenApiSchema schema)
    {
        // Une action qui documente elle-même ce statut l'emporte.
        if (operation.Responses!.ContainsKey(status))
            return;

        operation.Responses[status] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new() { Schema = schema },
            },
        };
    }
}
