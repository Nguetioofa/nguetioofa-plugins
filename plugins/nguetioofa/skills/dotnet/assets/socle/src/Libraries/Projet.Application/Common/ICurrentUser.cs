namespace Projet.Application.Common;

/// <summary>Compte et session de l'opération en cours ; tout à <c>null</c> pour le système ou une requête anonyme.</summary>
public interface ICurrentUser
{
    int? UserId { get; }

    long? SessionId { get; }

    int? DeviceId { get; }
}
