using Microsoft.OpenApi;
using Projet.API.Swagger;
using Projet.Application.Common;
using Projet.Domain.Common;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Projet.API.Extensions;

public static class SwaggerExtensions
{
    // La route exportée ne suit pas la version : chaque outil qui la lit devrait la suivre.
    private const string DocumentName = "v1";

    // La version publiée de l'API, montée à chaque changement de forme.
    private const string ContractVersion = "0.1.0";

    private const string Title = "Projet API";

    public static IServiceCollection AddApiSwagger(this IServiceCollection services) =>
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(DocumentName, new OpenApiInfo
            {
                Title = Title,
                Version = ContractVersion,
                Description =
                    "Toutes les réponses, succès comme échec, suivent l'enveloppe ApiResponse : " +
                    "tester « success », puis lire « data » ou « errorCode ». La langue des messages " +
                    "suit l'en-tête Accept-Language (fr par défaut, en).",
            });

            IncludeXmlComments(options);

            // En OpenAPI 3.0, seule une référence par allOf porte « nullable ».
            options.UseAllOfToExtendReferenceSchemas();

            // Sinon une requête liée par [FromQuery] publierait le nom C# (?PageNumber=).
            options.DescribeAllParametersInCamelCase();

            // Les versions (xmin) sont des entiers non signés sur 32 bits : au-delà de int32.
            options.MapType<uint>(() => new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64" });

            options.SchemaFilter<ResponseSchemaFilter>();
            options.OperationFilter<CommonResponsesOperationFilter>();
        });

    public static IApplicationBuilder UseApiSwagger(this IApplicationBuilder app) =>
        app.UseSwagger()
           .UseSwaggerUI(options =>
           {
               options.DocumentTitle = Title;
               options.SwaggerEndpoint($"/swagger/{DocumentName}/swagger.json", $"{Title} {ContractVersion}");
           });

    // Les trois couches : les types exposés (ApiResponse, PagedResult) vivent dans les bibliothèques.
    private static void IncludeXmlComments(SwaggerGenOptions options)
    {
        var assemblies = new[]
        {
            typeof(SwaggerExtensions).Assembly,
            typeof(SharedResource).Assembly,
            typeof(Error).Assembly,
        };

        foreach (var assembly in assemblies)
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
    }
}
