namespace Projet.Domain.Common;

/// <summary>Codes transverses, jamais renommés ; ceux d'un domaine se déclarent à côté de lui.</summary>
public static class ErrorCodes
{
    public const string NotFound = "ENTITY_NOT_FOUND";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string Conflict = "ENTITY_CONFLICT";
    public const string Duplicate = "ENTITY_DUPLICATE";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string Unexpected = "UNEXPECTED_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Cancelled = "REQUEST_CANCELLED";

    /// <summary>Un 429 du protocole, mais aussi un plafond métier : d'où sa place dans le domaine.</summary>
    public const string TooManyRequests = "TOO_MANY_REQUESTS";
}
