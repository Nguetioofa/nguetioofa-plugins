using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Projet.Application.Common;

namespace Projet.API.Extensions;

public static class LocalizationExtensions
{
    private static readonly CultureInfo[] SupportedCultures =
    [
        new CultureInfo("fr"),
        new CultureInfo("en"),
    ];

    /// <summary>Français par défaut, anglais ; un libellé d'énumération manquant empêche de démarrer.</summary>
    public static IServiceCollection AddApiLocalization(this IServiceCollection services)
    {
        EnumTranslations.EnsureComplete(SupportedCultures, neutralCulture: SupportedCultures[0]);

        // Projet.Application.Common.SharedResource est cherché sous Resources/Common/SharedResource.
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("fr");
            options.SupportedCultures = SupportedCultures;
            options.SupportedUICultures = SupportedCultures;

            // Accept-Language seul : ni cookie ni segment d'URL, qu'un client mobile ne produirait pas.
            options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        return services;
    }
}
