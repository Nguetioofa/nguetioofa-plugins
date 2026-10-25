namespace Projet.Application.Common;

/// <summary>Provenance de la requête en cours (journal d'audit) ; tout à <c>null</c> hors requête.</summary>
public interface IRequestInfo
{
    string? IpAddress { get; }

    string? UserAgent { get; }
}
