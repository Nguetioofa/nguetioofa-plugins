using System.Diagnostics;

namespace Projet.API.Extensions;

public static class HttpContextExtensions
{
    /// <summary>Identifiant de corrélation (W3C, celui des journaux), calculé ici seulement pour que 400 et 500 citent le même.</summary>
    public static string GetTraceId(this HttpContext context) =>
        Activity.Current?.Id ?? context.TraceIdentifier;
}
