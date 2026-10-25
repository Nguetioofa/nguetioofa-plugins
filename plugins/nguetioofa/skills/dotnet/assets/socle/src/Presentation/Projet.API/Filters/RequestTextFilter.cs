using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Projet.API.Validation;
using Projet.Application.Common;
using Projet.Application.Validation.Common;

namespace Projet.API.Filters;

/// <summary>
/// Remplace chaque texte de chaque argument par sa forme nettoyée (<see cref="RequestTexts"/>), avant les validateurs,
/// et refuse en 400 sous le champ celui qui porte encore un caractère non admis : rien de tel n'atteint la base.
/// </summary>
public sealed class RequestTextFilter(IOptions<ApiBehaviorOptions> behavior, IStringLocalizer<SharedResource> localizer) : IActionFilter
{
    private readonly ApiBehaviorOptions _behavior = behavior.Value;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var failures = new ModelStateDictionary();
        // Une copie des noms : un texte seul est remplacé dans la collection pendant le parcours.
        foreach (var name in context.ActionArguments.Keys.ToList())
        {
            var room = ValidationErrorLimits.MaxFields - failures.ErrorCount;
            if (room <= 0)
                break;

            var argument = context.ActionArguments[name];
            var (kept, refused) = RequestTexts.Clean(argument, name, KindOf(context, name), room);
            foreach (var (key, kind) in refused)
                failures.AddModelError(key, localizer[MessageKey(kind)]);

            if (!ReferenceEquals(kept, argument))
                context.ActionArguments[name] = kept;
        }

        if (!failures.IsValid)
            context.Result = ValidationFailures.Respond(context, failures, _behavior);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public static string MessageKey(TextKind kind) => kind switch
    {
        TextKind.Multiline => "Validation.TextControlCharacter",
        TextKind.Password => "Validation.InvalidFormat",
        _ => "Validation.TextLineBreakOrControl",
    };

    private static TextKind KindOf(ActionExecutingContext context, string name) =>
        context.ActionDescriptor.Parameters
            .OfType<ControllerParameterDescriptor>()
            .FirstOrDefault(parameter => parameter.Name == name)
            ?.ParameterInfo.IsDefined(typeof(MultilineTextAttribute), inherit: false) == true
            ? TextKind.Multiline
            : TextKind.Line;
}
