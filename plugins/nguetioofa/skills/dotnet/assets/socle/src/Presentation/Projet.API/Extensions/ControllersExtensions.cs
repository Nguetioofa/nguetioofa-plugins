using Projet.API.Filters;
using Projet.API.Json;
using Projet.API.ModelBinding;
using Projet.Application.Common;

namespace Projet.API.Extensions;

public static class ControllersExtensions
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                // Globaux, textes avant validateurs (à ordre égal, l'ordre d'inscription) : aucune action n'y échappe.
                options.Filters.Add<RequestTextFilter>();
                options.Filters.Add<FluentValidationFilter>();

                // Liaison stricte de l'adresse, en tête des fournisseurs : énumérations, puis (inséré devant) valeur répétée.
                options.ModelBinderProviders.Insert(0, new StrictEnumModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new SingleValueModelBinderProvider(options.ModelBinderProviders));
            })
            .AddDataAnnotationsLocalization(options =>
                options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource)))
            .AddJsonOptions(options =>
            {
                // Le message du sérialiseur nomme les types internes ; sans lui, ModelStateErrors traduit l'erreur.
                options.AllowInputFormatterExceptionMessages = false;
                options.JsonSerializerOptions.Converters.Add(new StrictEnumConverterFactory());
            });

        return services;
    }

    public static IEndpointRouteBuilder MapApiControllers(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        return endpoints;
    }
}
