namespace Projet.API.Errors;

/// <summary>Codes de la seule façade HTTP ; <c>ROUTE_NOT_FOUND</c> : l'appel est faux, pas la donnée (≠ <c>ENTITY_NOT_FOUND</c>).</summary>
public static class ProtocolErrorCodes
{
    public const string RouteNotFound = "ROUTE_NOT_FOUND";
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string NotAcceptable = "NOT_ACCEPTABLE";
    public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE";
    public const string RequestRejected = "REQUEST_REJECTED";
}
