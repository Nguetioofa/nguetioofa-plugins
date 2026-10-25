namespace Projet.Domain.Common;

/// <summary>Appartient à une organisation sans exception ; une table où elle est facultative ne porte pas cette marque.</summary>
public interface ITenantScoped
{
    int OrganizationId { get; }
}
