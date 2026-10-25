namespace Projet.Domain.Common;

/// <summary>Horodatée par la persistance ; une ligne jamais modifiée n'a que <c>CreatedAt</c> et ne porte pas cette marque.</summary>
public interface IHasTimestamps
{
    DateTime CreatedAt { get; set; }

    DateTime? UpdatedAt { get; set; }
}
