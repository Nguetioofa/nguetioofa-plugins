using Projet.API.Context;
using Projet.Application.Common;

namespace Projet.API.Extensions;

public static class RequestContextExtensions
{
    // Ici et non dans un module Autofac, qui passerait après et l'emporterait.
    public static IServiceCollection AddApiRequestContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUser, HttpCurrentUser>();
        services.AddSingleton<IRequestInfo, HttpRequestInfo>();

        return services;
    }
}
