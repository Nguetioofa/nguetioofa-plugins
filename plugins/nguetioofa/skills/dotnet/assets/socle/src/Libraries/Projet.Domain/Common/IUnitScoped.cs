namespace Projet.Domain.Common;

/// <summary>Appartient à une unité de l'organisation, liée en base par la clé composite <c>(OrganizationId, UnitId)</c>.</summary>
public interface IUnitScoped : ITenantScoped
{
    int UnitId { get; }
}
