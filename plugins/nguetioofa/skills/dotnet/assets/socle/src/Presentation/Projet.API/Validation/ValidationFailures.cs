using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Projet.API.Validation;

/// <summary>Les refus d'un filtre, dans un <c>ModelState</c> à part : celui de la requête garderait la casse d'un paramètre de route homonyme.</summary>
public static class ValidationFailures
{
    public static IActionResult Respond(ActionContext context, ModelStateDictionary failures, ApiBehaviorOptions behavior)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentNullException.ThrowIfNull(behavior);

        return behavior.InvalidModelStateResponseFactory(
            new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor, failures));
    }
}
