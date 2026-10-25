namespace Projet.Application.Common;

/// <summary>L'heure UTC à la milliseconde, la précision de la base ; une date reçue sans fuseau est lue comme de l'UTC.</summary>
public static class UtcTime
{
    public static DateTime Now() => Truncate(DateTime.UtcNow);

    public static DateTime FromClient(DateTime value) => Truncate(value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    });

    private static DateTime Truncate(DateTime value) =>
        value.AddTicks(-(value.Ticks % TimeSpan.TicksPerMillisecond));
}
