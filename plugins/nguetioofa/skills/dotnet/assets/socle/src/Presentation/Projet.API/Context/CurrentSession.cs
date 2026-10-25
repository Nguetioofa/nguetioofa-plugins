namespace Projet.API.Context;

/// <summary>Compte et session de la requête, posés dans <c>HttpContext.Features</c> par l'authentification ; absents, la requête est anonyme.</summary>
public sealed record CurrentSession(int UserId, long SessionId, int DeviceId);
