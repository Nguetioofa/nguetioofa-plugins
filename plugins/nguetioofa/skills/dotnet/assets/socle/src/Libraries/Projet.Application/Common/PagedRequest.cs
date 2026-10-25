namespace Projet.Application.Common;

/// <summary>Base des requêtes paginées : une taille non bornée ferait d'un appel anodin un export de la table.</summary>
public abstract class PagedRequest
{
    public const int MaxPageSize = 200;
    public const int DefaultPageSize = 20;

    /// <summary>Au-delà, page × taille ne tiendrait plus dans un entier.</summary>
    public const int MaxPageNumber = 1_000_000;

    private int _pageSize = DefaultPageSize;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > MaxPageSize ? DefaultPageSize : value;
    }

    public string? SearchTerm { get; set; }

    public string? SortBy { get; set; }

    public bool SortDescending { get; set; }

    // Une méthode : une propriété serait publiée comme paramètre de requête (?skip=).
    public int ItemsToSkip() => ItemsToSkip(PageNumber, PageSize);

    public static int ItemsToSkip(int pageNumber, int pageSize) =>
        (int)Math.Clamp((Math.Max(pageNumber, 1) - 1L) * Math.Max(pageSize, 0), 0L, int.MaxValue);
}
