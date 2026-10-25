namespace Projet.Domain.Common;

/// <summary>Horodatée et attribuée (<c>null</c> pour le système) ; les quatre colonnes vont ensemble.</summary>
public interface IAuditable : IHasTimestamps
{
    int? CreatedBy { get; set; }

    int? UpdatedBy { get; set; }
}
