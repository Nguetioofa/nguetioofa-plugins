namespace Projet.Application.Common;

/// <summary>Plafond des erreurs d'une réponse : une demande fausse partout ne reçoit pas cent mille messages.</summary>
public static class ValidationErrorLimits
{
    public const int MaxFields = 200;

    public const int MaxMessagesPerField = 10;

    public static Dictionary<string, string[]> Cap(IEnumerable<KeyValuePair<string, string[]>> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return errors
            .Take(MaxFields)
            .ToDictionary(
                error => error.Key,
                error => error.Value.Length <= MaxMessagesPerField ? error.Value : error.Value[..MaxMessagesPerField]);
    }
}
