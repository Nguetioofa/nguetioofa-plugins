using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Projet.API.Errors;
using Projet.API.Extensions;
using Projet.Application.Common;
using Projet.Domain.Common;

namespace Projet.API.Middleware;

public static class StatusCodePagesExtensions
{
    public static IServiceCollection AddApiStatusCodePages(this IServiceCollection services)
    {
        // MVC remplirait ses refus sans corps (415, 404 nu) de son propre format : ils partent vides, et reçoivent l'enveloppe ici.
        services.Configure<ApiBehaviorOptions>(options => options.SuppressMapClientErrors = true);

        services.TryAddSingleton<ApiResponseWriter>();

        return services;
    }

    /// <summary>Donne l'enveloppe à toute réponse 400 à 599 partie sans corps (routage, MVC avant l'action).</summary>
    public static IApplicationBuilder UseApiStatusCodePages(this IApplicationBuilder app) =>
        app.UseStatusCodePages(async statusContext =>
        {
            var context = statusContext.HttpContext;
            var localizer = context.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
            var writer = context.RequestServices.GetRequiredService<ApiResponseWriter>();

            var status = context.Response.StatusCode;
            var (errorCode, messageKey) = Describe(context, status);

            var error = Error.Business(errorCode, localizer[messageKey], status);

            await writer.WriteAsync(context, ApiResponse.FromError<object>(error, context.GetTraceId()));
        });

    private static (string ErrorCode, string MessageKey) Describe(HttpContext context, int status) => status switch
    {
        StatusCodes.Status400BadRequest => (ErrorCodes.ValidationFailed, "Error.ValidationFailed"),
        StatusCodes.Status401Unauthorized => (ErrorCodes.Unauthorized, "Error.Unauthorized"),
        StatusCodes.Status403Forbidden => (ErrorCodes.Forbidden, "Error.Forbidden"),

        // Sans point d'entrée, c'est l'adresse qui est fausse ; avec, une action a rendu un 404 nu.
        StatusCodes.Status404NotFound => context.GetEndpoint() is null
            ? (ProtocolErrorCodes.RouteNotFound, "Error.RouteNotFound")
            : (ErrorCodes.NotFound, "Error.NotFound"),

        StatusCodes.Status405MethodNotAllowed => (ProtocolErrorCodes.MethodNotAllowed, "Error.MethodNotAllowed"),
        StatusCodes.Status406NotAcceptable => (ProtocolErrorCodes.NotAcceptable, "Error.NotAcceptable"),
        StatusCodes.Status415UnsupportedMediaType => (ProtocolErrorCodes.UnsupportedMediaType, "Error.UnsupportedMediaType"),
        StatusCodes.Status429TooManyRequests => (ErrorCodes.TooManyRequests, "Error.TooManyRequests"),
        >= StatusCodes.Status500InternalServerError => (ErrorCodes.Unexpected, "Error.Unexpected"),
        _ => (ProtocolErrorCodes.RequestRejected, "Error.RequestRejected"),
    };
}
