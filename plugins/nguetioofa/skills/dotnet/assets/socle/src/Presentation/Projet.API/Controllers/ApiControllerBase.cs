using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Projet.API.Extensions;
using Projet.Application.Common;
using Projet.Domain.Common;

namespace Projet.API.Controllers;

/// <summary>Socle des contrôleurs : traduire un <see cref="Result{T}"/> en réponse, le statut venant de l'erreur.</summary>
/// <remarks>⚠ Aucun <c>[ProducesResponseType]</c> ici : il ferait disparaître le 200 déduit du type de retour de chaque action.</remarks>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string TraceId => HttpContext.GetTraceId();

    protected ActionResult<ApiResponse<T>> Respond<T>(Result<T> result) =>
        result.Match(
            value => StatusCode(StatusCodes.Status200OK, ApiResponse<T>.Ok(value, TraceId)),
            Failure<T>);

    protected ActionResult<ApiResponse<T>> RespondCreated<T>(Result<T> result) =>
        result.Match(
            value => StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Created(value, TraceId)),
            Failure<T>);

    /// <summary>200 et <c>data</c> vide plutôt qu'un 204 : une seule forme de réponse.</summary>
    protected ActionResult<ApiResponse<object?>> Respond(Result result) =>
        result.Match(
            () => StatusCode(StatusCodes.Status200OK, ApiResponse<object?>.Ok(null, TraceId)),
            Failure<object?>);

    /// <summary>Un fichier en cas de succès, seul cas hors de l'enveloppe ; l'échec y reste.</summary>
    protected ActionResult RespondFile<T>(Result<T> result, Func<T, FileResult> toFile)
    {
        ArgumentNullException.ThrowIfNull(toFile);
        return result.Match<ActionResult>(value => toFile(value), Failure<object?>);
    }

    private ObjectResult Failure<T>(Error error)
    {
        if (error.RetryAfter is { } retryAfter)
            Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

        return StatusCode(error.Status, ApiResponse.FromError<T>(error, TraceId));
    }
}
