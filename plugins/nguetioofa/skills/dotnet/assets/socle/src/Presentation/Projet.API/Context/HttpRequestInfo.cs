using Projet.Application.Common;

namespace Projet.API.Context;

public sealed class HttpRequestInfo(IHttpContextAccessor accessor) : IRequestInfo
{
    public string? IpAddress
    {
        get
        {
            var address = accessor.HttpContext?.Connection.RemoteIpAddress;

            return address is null ? null : (address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address).ToString();
        }
    }

    // Un en-tête échappe au filtre des demandes : jugé ici, et non gardé s'il est refusé (une trace ne fait pas échouer l'action).
    public string? UserAgent
    {
        get
        {
            var value = accessor.HttpContext?.Request.Headers.UserAgent.ToString();
            return value is not null && TextRules.TryClean(TextKind.Line, value, out var clean) && clean.Length > 0 ? clean : null;
        }
    }
}
