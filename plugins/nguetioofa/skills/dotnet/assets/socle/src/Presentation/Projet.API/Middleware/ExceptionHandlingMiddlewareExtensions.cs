using Microsoft.Extensions.DependencyInjection.Extensions;
using Projet.API.Extensions;

namespace Projet.API.Middleware;

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
    {
        // Sans état. TryAdd : les pages de statut le déclarent aussi.
        services.TryAddSingleton<ApiResponseWriter>();

        return services;
    }

    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
