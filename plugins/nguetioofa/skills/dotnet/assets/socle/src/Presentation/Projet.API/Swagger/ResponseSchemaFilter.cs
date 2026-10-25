using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using Projet.Application.Common;
using Projet.Domain.Common;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Projet.API.Swagger;

/// <summary>Schémas de <b>réponse</b> exacts (DTO, page, enveloppe) : toute propriété toujours écrite est <c>required</c>, et
/// <c>nullable</c> seulement si son type C# l'est ; sans lui, le client généré rendrait tout facultatif.</summary>
/// <remarks>Les requêtes ne sont pas touchées : les durcir serait un changement cassant pour le client.</remarks>
public sealed class ResponseSchemaFilter : ISchemaFilter
{
    private const string ResponseNamespace = "Projet.Application.DTOs";

    private readonly NullabilityInfoContext _nullability = new();

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!IsResponse(context.Type) || schema is not OpenApiSchema target || target.Properties is not { Count: > 0 } properties)
            return;

        foreach (var member in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!properties.TryGetValue(JsonName(member), out var property))
                continue;

            if (IsAlwaysWritten(member))
            {
                target.Required ??= new HashSet<string>();
                target.Required.Add(JsonName(member));
            }

            if (property is OpenApiSchema propertySchema)
                SetNullable(propertySchema, IsNullable(member));
        }
    }

    // PagedResult<T> vit dans le domaine, hors des DTO.
    private static bool IsResponse(Type type) =>
        type.Namespace?.StartsWith(ResponseNamespace, StringComparison.Ordinal) == true
        || (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ApiResponse<>) || type.GetGenericTypeDefinition() == typeof(PagedResult<>)));

    private static string JsonName(PropertyInfo member) =>
        member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? JsonNamingPolicy.CamelCase.ConvertName(member.Name);

    private static bool IsAlwaysWritten(PropertyInfo member) =>
        member.GetCustomAttribute<JsonExtensionDataAttribute>() is null
        && member.GetCustomAttribute<JsonIgnoreAttribute>() is not { Condition: JsonIgnoreCondition.WhenWritingNull or JsonIgnoreCondition.WhenWritingDefault or JsonIgnoreCondition.Always };

    // La nullabilité est en erreur dans tout le projet : l'annotation « ? » est fiable.
    private bool IsNullable(PropertyInfo member) =>
        Nullable.GetUnderlyingType(member.PropertyType) is not null
        || (!member.PropertyType.IsValueType && _nullability.Create(member).ReadState == NullabilityState.Nullable);

    // OpenAPI 3.0 : « nullable: true » s'écrit à partir du drapeau Null du type.
    private static void SetNullable(OpenApiSchema property, bool nullable)
    {
        if (nullable)
            property.Type = (property.Type ?? 0) | JsonSchemaType.Null;
        else if (property.Type is { } type)
            property.Type = (type & ~JsonSchemaType.Null) is var rest && rest != 0 ? rest : null;
    }
}
