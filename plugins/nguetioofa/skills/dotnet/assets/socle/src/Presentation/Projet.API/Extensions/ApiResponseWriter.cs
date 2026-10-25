using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Projet.Application.Common;

namespace Projet.API.Extensions;

/// <summary>Écrit l'enveloppe hors de MVC exactement comme un contrôleur : options JSON de MVC, encodeur permissif compris.</summary>
public sealed class ApiResponseWriter
{
    private readonly JsonSerializerOptions _serializerOptions;

    public ApiResponseWriter(IOptions<JsonOptions> jsonOptions)
    {
        var mvcOptions = jsonOptions.Value.JsonSerializerOptions;
        _serializerOptions = mvcOptions.Encoder is null
            ? new JsonSerializerOptions(mvcOptions) { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
            : mvcOptions;
    }

    /// <remarks>Jamais <c>Response.Clear()</c> : il efface CORS et <c>Content-Language</c>, et le navigateur ne laisserait plus lire l'erreur.</remarks>
    public Task WriteAsync<T>(HttpContext context, ApiResponse<T> response)
    {
        context.Response.StatusCode = response.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentLength = null;

        return context.Response.WriteAsJsonAsync(
            response,
            _serializerOptions,
            contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }
}
