using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Projet.API.Extensions;

public static class RequestLimitsExtensions
{
    /// <summary>Corps de requête au plus, hors routes qui déclarent <c>[RequestSizeLimit]</c> : au-delà, 413 dans l'enveloppe, avant toute désérialisation.</summary>
    public const long MaxBodyBytes = 2 * 1024 * 1024;

    public static IServiceCollection AddApiRequestLimits(this IServiceCollection services)
    {
        services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = MaxBodyBytes);
        return services;
    }
}
