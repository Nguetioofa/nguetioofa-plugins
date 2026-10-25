using Microsoft.Extensions.Localization;
using Projet.API.Extensions;
using Projet.Application.Common;
using Projet.Domain.Common;

namespace Projet.API.Middleware;

/// <summary>Convertit toute exception non rattrapée en enveloppe ; les issues attendues passent par <see cref="Result{T}"/>.</summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment,
    ApiResponseWriter writer)
{
    public async Task InvokeAsync(HttpContext context, IStringLocalizer<SharedResource> localizer)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception, localizer);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception, IStringLocalizer<SharedResource> localizer)
    {
        var traceId = context.GetTraceId();

        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            // Le client est parti : personne à qui répondre, et écrire lèverait une seconde exception.
            logger.LogInformation(
                "Requête {Method} {Path} abandonnée par le client — trace {TraceId}",
                context.Request.Method, LoggedRequestPath.Of(context), traceId);

            if (!context.Response.HasStarted)
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;

            return;
        }

        // Avec EF Core : DbUpdateConcurrencyException => (409, ErrorCodes.ConcurrencyConflict, "Error.Concurrency"), clé fr + en.
        var (status, errorCode, messageKey) = exception switch
        {
            // Refusée par Kestrel avant le contrôleur (corps trop gros…) : la faute de l'appelant, statut porté par l'exception.
            BadHttpRequestException badRequest =>
                (badRequest.StatusCode, ErrorCodes.ValidationFailed, "Error.ValidationFailed"),

            _ => (StatusCodes.Status500InternalServerError, ErrorCodes.Unexpected, "Error.Unexpected"),
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception,
                "Requête {Method} {Path} en échec — trace {TraceId}",
                context.Request.Method, LoggedRequestPath.Of(context), traceId);
        }
        else
        {
            logger.LogWarning(
                "Requête {Method} {Path} refusée : {Reason} — trace {TraceId}",
                context.Request.Method, LoggedRequestPath.Of(context), errorCode, traceId);
        }

        if (context.Response.HasStarted)
        {
            // La réécrire lèverait une seconde exception et masquerait la première.
            logger.LogWarning("Réponse déjà commencée, l'erreur ne peut plus être renvoyée au client — trace {TraceId}", traceId);
            return;
        }

        // Le texte technique ne sort qu'en développement et pour l'imprévu (≥ 500) : un cas prévu garde son message traduit.
        var error = Error.Business(
            errorCode,
            environment.IsDevelopment() && status >= StatusCodes.Status500InternalServerError
                ? exception.Message
                : localizer[messageKey],
            status);

        var response = ApiResponse.FromError<object>(error, traceId);

        if (environment.IsDevelopment())
        {
            response.Metadata = new Dictionary<string, object>
            {
                ["exception"] = exception.GetType().FullName ?? exception.GetType().Name,
                ["stackTrace"] = exception.StackTrace ?? string.Empty,
            };
        }

        await writer.WriteAsync(context, response);
    }
}
