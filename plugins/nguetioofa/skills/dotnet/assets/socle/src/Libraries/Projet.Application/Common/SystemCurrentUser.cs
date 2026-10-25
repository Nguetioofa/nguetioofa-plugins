namespace Projet.Application.Common;

/// <summary>Le système (tâche de fond, script) : une recherche sur ce nom montre chaque écriture sans compte.</summary>
public sealed class SystemCurrentUser : ICurrentUser
{
    public static readonly SystemCurrentUser Instance = new();

    private SystemCurrentUser()
    {
    }

    public int? UserId => null;

    public long? SessionId => null;

    public int? DeviceId => null;
}
