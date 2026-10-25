using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Projet.Domain.Common;

namespace Projet.Application.Common;

/// <summary>Enveloppe de toutes les réponses, succès comme échec : tester <c>success</c>, puis lire <c>data</c> ou <c>errorCode</c>.</summary>
/// <remarks>Dérive de <see cref="ProblemDetails"/> (RFC 9457) ; <c>title</c> est technique, le texte traduit est dans <c>detail</c>.</remarks>
public class ApiResponse<T> : ProblemDetails
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>Code d'erreur stable : c'est lui que le client teste, jamais le message.</summary>
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    /// <summary>Identifiant de corrélation, à citer dans un rapport d'incident.</summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("validationErrors")]
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    public static ApiResponse<T> Ok(T data, string? traceId = null) => new()
    {
        Success = true,
        Data = data,
        Status = StatusCodes.Status200OK,
        Title = "Success",
        TraceId = traceId
    };

    public static ApiResponse<T> Created(T data, string? traceId = null) => new()
    {
        Success = true,
        Data = data,
        Status = StatusCodes.Status201Created,
        Title = "Created",
        TraceId = traceId
    };

    public static ApiResponse<T> ValidationFailed(Dictionary<string, string[]> errors, string detail, string? traceId = null) => new()
    {
        Success = false,
        Status = StatusCodes.Status400BadRequest,
        Title = "Validation Error",
        Detail = detail,
        ErrorCode = ErrorCodes.ValidationFailed,
        ValidationErrors = ValidationErrorLimits.Cap(errors),
        TraceId = traceId
    };

    public static ApiResponse<T> Failure(string detail, string? traceId = null) => new()
    {
        Success = false,
        Status = StatusCodes.Status500InternalServerError,
        Title = "Internal Server Error",
        Detail = detail,
        ErrorCode = ErrorCodes.Unexpected,
        TraceId = traceId
    };
}

/// <summary>Seul point de traduction d'une issue de service vers l'enveloppe ; le statut vient de l'erreur.</summary>
public static class ApiResponse
{
    public static ApiResponse<T> From<T>(Result<T> result, string? traceId = null) =>
        result.Match(
            value => ApiResponse<T>.Ok(value, traceId),
            error => FromError<T>(error, traceId));

    public static ApiResponse<T> FromError<T>(Error error, string? traceId = null) => new()
    {
        Success = false,
        Status = error.Status,
        Title = TitleFor(error.Status),
        Detail = error.Message,
        ErrorCode = error.Code,
        TraceId = traceId,
        // Plafonnées comme celles d'un validateur : un service peut en rendre autant que la demande a de lignes.
        ValidationErrors = error.ValidationErrors is { } errors ? ValidationErrorLimits.Cap(errors) : null
    };

    // La table du framework couvre tous les statuts (405, 415, 429…), une liste tenue à la main en oublierait.
    private static string TitleFor(int status)
    {
        var phrase = ReasonPhrases.GetReasonPhrase(status);
        return string.IsNullOrEmpty(phrase) ? "Error" : phrase;
    }
}
