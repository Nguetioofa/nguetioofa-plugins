namespace Projet.Domain.Common;

/// <summary>Un échec : code stable (jamais un enum), message déjà traduit, statut HTTP.</summary>
/// <remarks>Le statut est porté ici : seul le domaine sait <em>pourquoi</em> l'opération a échoué, la couche HTTP le recopie.</remarks>
public sealed record Error
{
    public required string Code { get; init; }

    public required string Message { get; init; }

    public int Status { get; init; } = 400;

    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    /// <summary>Délai avant un nouvel essai, rendu dans l'en-tête <c>Retry-After</c>.</summary>
    public TimeSpan? RetryAfter { get; init; }

    public static Error NotFound(string message) =>
        new() { Code = ErrorCodes.NotFound, Message = message, Status = 404 };

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors, string message) =>
        new() { Code = ErrorCodes.ValidationFailed, Message = message, Status = 400, ValidationErrors = errors };

    /// <summary>Refus sans champ à désigner : le ranger sous un champ inventé tromperait l'écran.</summary>
    public static Error Validation(string message) =>
        new() { Code = ErrorCodes.ValidationFailed, Message = message, Status = 400 };

    public static Error Conflict(string message) =>
        new() { Code = ErrorCodes.Conflict, Message = message, Status = 409 };

    public static Error Duplicate(string message) =>
        new() { Code = ErrorCodes.Duplicate, Message = message, Status = 409 };

    public static Error Business(string code, string message, int status = 400) =>
        new() { Code = code, Message = message, Status = status };

    public static Error Unauthorized(string message) =>
        new() { Code = ErrorCodes.Unauthorized, Message = message, Status = 401 };

    public static Error Forbidden(string message) =>
        new() { Code = ErrorCodes.Forbidden, Message = message, Status = 403 };

    public static Error TooManyRequests(string message, TimeSpan? retryAfter = null) =>
        new() { Code = ErrorCodes.TooManyRequests, Message = message, Status = 429, RetryAfter = retryAfter };

    public static Error Unexpected(string message) =>
        new() { Code = ErrorCodes.Unexpected, Message = message, Status = 500 };

    public override string ToString() => $"{Code} : {Message}";
}
