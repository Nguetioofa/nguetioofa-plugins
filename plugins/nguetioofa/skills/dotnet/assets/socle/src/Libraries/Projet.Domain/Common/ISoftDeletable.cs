namespace Projet.Domain.Common;

/// <summary>Supprimée logiquement ; lever son filtre par son nom (<c>IgnoreQueryFilters(["SoftDelete"])</c>), jamais sans argument.</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    DateTime? DeletedAt { get; set; }

    int? DeletedBy { get; set; }

    string? DeletionReason { get; set; }
}
