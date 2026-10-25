using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Projet.Application.Common;

namespace Projet.API.Validation;

/// <summary>Le <c>ModelState</c> en erreurs par champ ; celles de lecture du JSON (anglais, types internes, chemin <c>$.</c>) réécrites.</summary>
public static class ModelStateErrors
{
    public const string BodyKey = "";

    public static Dictionary<string, string[]> ToResponseErrors(ActionContext context, IStringLocalizer<SharedResource> localizer)
    {
        var unreadBodyParameters = UnreadBodyParameters(context);
        var unreadForm = UnreadForm(context, localizer);
        var errors = new Dictionary<string, List<string>>();

        foreach (var (key, entry) in context.ModelState)
        {
            if (entry.Errors.Count == 0)
                continue;

            // Un formulaire illisible ne laisse que son message brut, en anglais, sous la clé vide.
            if (unreadForm is not null && key == BodyKey)
            {
                errors[BodyKey] = [unreadForm];
                continue;
            }

            // Conséquence d'un corps illisible, sous le nom C# du paramètre : la cause figure déjà sous une autre clé.
            if (unreadBodyParameters.Contains(key))
                continue;

            foreach (var error in entry.Errors)
            {
                var (responseKey, message) = Translate(key, error, localizer);

                if (!errors.TryGetValue(responseKey, out var messages))
                    errors[responseKey] = messages = [];

                if (!messages.Contains(message))
                    messages.Add(message);
            }
        }

        return errors.ToDictionary(e => e.Key, e => e.Value.ToArray());
    }

    private static (string Key, string Message) Translate(string key, ModelError error, IStringLocalizer<SharedResource> localizer)
    {
        // Un chemin JSON (« $… ») : l'erreur est survenue en lisant le corps, avant toute validation.
        if (error.Exception is JsonException || key.StartsWith('$'))
        {
            return key is "$" or ""
                ? (BodyKey, localizer["Validation.InvalidJson"])
                : (ToPropertyKey(key), localizer["Validation.InvalidFormat"]);
        }

        // Jamais de chaîne vide au client, ni le texte d'une exception.
        var message = string.IsNullOrEmpty(error.ErrorMessage)
            ? localizer["Error.ValidationFailed"]
            : error.ErrorMessage;

        return (key, message);
    }

    /// <summary><c>$.adresse.codePostal</c> → <c>Adresse.CodePostal</c>, la forme des clés des validateurs.</summary>
    private static string ToPropertyKey(string jsonPath)
    {
        var path = jsonPath.StartsWith("$.") ? jsonPath[2..] : jsonPath.TrimStart('$');

        return string.Join('.', path
            .Split('.', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => char.ToUpperInvariant(segment[0]) + segment[1..]));
    }

    private static string? UnreadForm(ActionContext context, IStringLocalizer<SharedResource> localizer)
    {
        var request = context.HttpContext.Request;
        if (!request.HasFormContentType || context.HttpContext.Features.Get<IFormFeature>()?.Form is not null)
            return null;

        var limit = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>()?.MaxRequestBodySize;
        return request.ContentLength > limit
            ? localizer["Validation.RequestTooLarge"]
            : localizer["Validation.InvalidForm"];
    }

    /// <summary>Les paramètres <c>[FromBody]</c> restés vides ; appelée par un filtre, l'argument existe et son erreur est gardée.</summary>
    private static HashSet<string> UnreadBodyParameters(ActionContext context)
    {
        if (context is not ActionExecutingContext executing)
            return [];

        return context.ActionDescriptor.Parameters
            .Where(p => p.BindingInfo?.BindingSource == BindingSource.Body)
            .Where(p => !executing.ActionArguments.TryGetValue(p.Name, out var value) || value is null)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);
    }
}
