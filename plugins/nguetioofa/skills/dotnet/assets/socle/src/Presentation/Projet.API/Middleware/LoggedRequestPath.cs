namespace Projet.API.Middleware;

/// <summary>L'adresse telle qu'elle s'écrit au journal : une route à secret (<c>…/{token}/accept</c>) par son modèle seulement.</summary>
public static class LoggedRequestPath
{
    private static readonly string[] SecretParameters = ["token"];

    public static string Of(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.GetEndpoint() is RouteEndpoint { RoutePattern: var pattern }
            && pattern.Parameters.Any(p => SecretParameters.Contains(p.Name, StringComparer.OrdinalIgnoreCase)))
            return "/" + (pattern.RawText ?? string.Empty).TrimStart('/');

        return context.Request.Path.ToString();
    }
}
