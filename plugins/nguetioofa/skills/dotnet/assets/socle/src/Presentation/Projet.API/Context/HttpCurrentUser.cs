using Projet.Application.Common;

namespace Projet.API.Context;

/// <summary>Le compte de la requête HTTP en cours ; tout à <c>null</c> pour une requête anonyme ou hors requête.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public int? UserId => Session?.UserId;

    public long? SessionId => Session?.SessionId;

    public int? DeviceId => Session?.DeviceId;

    private CurrentSession? Session => accessor.HttpContext?.Features.Get<CurrentSession>();
}
