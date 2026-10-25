using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Projet.API.Validation;
using Projet.Application.Common;

namespace Projet.API.Extensions;

public static class ValidationExtensions
{
    /// <summary>Erreurs de validation traduites, dans l'enveloppe.</summary>
    /// <remarks>Par <c>PostConfigure</c> : un <c>Configure</c> ne battrait les défauts de MVC qu'appelé après <c>AddControllers</c>.</remarks>
    public static IServiceCollection AddApiValidationErrors(this IServiceCollection services)
    {
        services.AddOptions<MvcOptions>()
            .PostConfigure<IStringLocalizerFactory>(TranslateModelBindingMessages);

        // Après le fournisseur des annotations, qui pose le [Required] implicite : sans lui, « "x": null » répondrait en anglais.
        services.PostConfigure<MvcOptions>(options =>
            options.ModelMetadataDetailsProviders.Add(new ImplicitRequiredMessageProvider()));

        // Même un modèle invalide sort dans l'enveloppe : le client n'a qu'une forme d'erreur.
        services.PostConfigure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = ValidationFailedResponse);

        return services;
    }

    // Tous les refus de format disent la même chose, sans répéter la valeur reçue.
    private static void TranslateModelBindingMessages(MvcOptions options, IStringLocalizerFactory factory)
    {
        var localizer = factory.Create(typeof(SharedResource));
        var messages = options.ModelBindingMessageProvider;

        messages.SetMissingRequestBodyRequiredValueAccessor(() => localizer["Validation.RequestBodyRequired"]);
        messages.SetMissingBindRequiredValueAccessor(_ => localizer["Validation.Required"]);
        messages.SetMissingKeyOrValueAccessor(() => localizer["Validation.Required"]);

        messages.SetAttemptedValueIsInvalidAccessor((_, _) => localizer["Validation.InvalidFormat"]);
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => localizer["Validation.InvalidFormat"]);
        messages.SetUnknownValueIsInvalidAccessor(_ => localizer["Validation.InvalidFormat"]);
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => localizer["Validation.InvalidFormat"]);
        messages.SetValueIsInvalidAccessor(_ => localizer["Validation.InvalidFormat"]);
        messages.SetValueMustBeANumberAccessor(_ => localizer["Validation.InvalidFormat"]);
        messages.SetNonPropertyValueMustBeANumberAccessor(() => localizer["Validation.InvalidFormat"]);
        messages.SetValueMustNotBeNullAccessor(_ => localizer["Validation.Required"]);
    }

    private static IActionResult ValidationFailedResponse(ActionContext context)
    {
        var localizer = context.HttpContext.RequestServices
            .GetRequiredService<IStringLocalizer<SharedResource>>();

        var response = ApiResponse<object>.ValidationFailed(
            ModelStateErrors.ToResponseErrors(context, localizer),
            localizer["Error.ValidationFailed"],
            context.HttpContext.GetTraceId());

        return new BadRequestObjectResult(response);
    }
}
