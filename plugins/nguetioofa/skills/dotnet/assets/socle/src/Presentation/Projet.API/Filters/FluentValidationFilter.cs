using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Projet.API.Validation;

namespace Projet.API.Filters;

/// <summary>Exécute les validateurs des arguments ; leurs refus sortent par la fabrique du démarrage, comme ceux de la liaison.</summary>
public sealed class FluentValidationFilter(IServiceProvider services, IOptions<ApiBehaviorOptions> behavior) : IAsyncActionFilter
{
    private readonly ApiBehaviorOptions _behavior = behavior.Value;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var failures = new ModelStateDictionary();
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            foreach (var failure in result.Errors)
            {
                // Le nom de la propriété, comme la liaison ; une faute sur l'objet entier prend la clé du corps.
                var key = string.IsNullOrEmpty(failure.PropertyName) ? ModelStateErrors.BodyKey : failure.PropertyName;
                failures.AddModelError(key, failure.ErrorMessage);
            }
        }

        if (!failures.IsValid && IsApi(context))
        {
            // Le filtre automatique de MVC a déjà tourné : la fabrique s'appelle ici.
            context.Result = ValidationFailures.Respond(context, failures, _behavior);
            return;
        }

        // Hors [ApiController], l'action décide.
        context.ModelState.Merge(failures);

        await next();
    }

    private static bool IsApi(ActionExecutingContext context) =>
        context.ActionDescriptor.EndpointMetadata.OfType<IApiBehaviorMetadata>().Any();
}
