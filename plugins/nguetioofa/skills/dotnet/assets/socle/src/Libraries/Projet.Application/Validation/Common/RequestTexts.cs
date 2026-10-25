using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Projet.Application.Common;

namespace Projet.Application.Validation.Common;

/// <summary>
/// Nettoie et juge (<see cref="TextRules"/>) <b>tous les textes</b> d'un argument d'action : chaque texte admis est
/// remplacé à sa place par sa forme nettoyée, la clé (<c>Lines[0].Code</c>) de chaque texte refusé est rendue.
/// </summary>
/// <remarks>
/// Seuls les types de cette couche (les demandes) et les listes sont parcourus. Un texte qui ne peut pas être
/// remplacé (sans setter, liste figée) est refusé dès que le nettoyage le changerait, jamais gardé tel quel.
/// </remarks>
public static class RequestTexts
{
    public const int MaxDepth = 64;

    private static readonly Assembly Requests = typeof(RequestTexts).Assembly;

    private static readonly ConcurrentDictionary<Type, (PropertyInfo Property, TextKind Kind)[]> Properties = new();

    /// <returns><c>Kept</c> : l'argument nettoyé sur place, ou le texte nettoyé ; <c>Refused</c> : les clés refusées.</returns>
    public static (object? Kept, IReadOnlyList<(string Key, TextKind Kind)> Refused) Clean(
        object? argument, string name, TextKind kind = TextKind.Line, int limit = ValidationErrorLimits.MaxFields)
    {
        ArgumentNullException.ThrowIfNull(name);

        var refused = new List<(string, TextKind)>();
        // Une demande rend les clés de ses champs, sans le nom C# de l'argument.
        var kept = Walk(argument, argument is string or IEnumerable ? name : string.Empty, kind, depth: 0, refused, limit);
        return (kept, refused);
    }

    private static object? Walk(object? value, string key, TextKind kind, int depth, List<(string, TextKind)> refused, int limit)
    {
        if (value is null || refused.Count >= limit || depth > MaxDepth)
            return value;

        if (value is string text)
        {
            if (TextRules.TryClean(kind, text, out var clean))
                return clean;

            refused.Add((key, kind));
            return value;
        }

        if (value is IEnumerable sequence)
        {
            WalkSequence(sequence, key, kind, depth, refused, limit);
            return value;
        }

        var type = value.GetType();
        if (type.Assembly != Requests)
            return value;

        foreach (var (property, propertyKind) in Properties.GetOrAdd(type, Read))
        {
            var propertyKey = key.Length == 0 ? property.Name : $"{key}.{property.Name}";
            var current = property.GetValue(value);
            var kept = Walk(current, propertyKey, propertyKind, depth + 1, refused, limit);
            if (ReferenceEquals(kept, current))
                continue;

            if (property.CanWrite)
                property.SetValue(value, kept);
            else
                refused.Add((propertyKey, propertyKind));
        }

        return value;
    }

    private static void WalkSequence(IEnumerable sequence, string key, TextKind kind, int depth, List<(string, TextKind)> refused, int limit)
    {
        // Un tableau se dit « en lecture seule » (il ne grandit pas), mais ses éléments s'écrivent.
        var texts = sequence as string[] ?? (sequence is IList<string> { IsReadOnly: false } list ? list : null);
        if (texts is not null)
        {
            for (var i = 0; i < texts.Count && refused.Count < limit; i++)
            {
                if (texts[i] is { } text && Walk(text, $"{key}[{i}]", kind, depth + 1, refused, limit) is string kept && !ReferenceEquals(kept, text))
                    texts[i] = kept;
            }

            return;
        }

        var index = 0;
        foreach (var item in sequence)
        {
            if (refused.Count >= limit)
                return;

            var itemKey = $"{key}[{index++}]";
            if (item is not null && !ReferenceEquals(Walk(item, itemKey, kind, depth + 1, refused, limit), item))
                refused.Add((itemKey, kind));
        }
    }

    private static (PropertyInfo Property, TextKind Kind)[] Read(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
            .Where(property => !property.PropertyType.IsValueType)
            .Select(property => (
                property,
                property.IsDefined(typeof(MultilineTextAttribute)) ? TextKind.Multiline
                : property.IsDefined(typeof(PasswordTextAttribute)) ? TextKind.Password
                : TextKind.Line))
            .ToArray();
}
