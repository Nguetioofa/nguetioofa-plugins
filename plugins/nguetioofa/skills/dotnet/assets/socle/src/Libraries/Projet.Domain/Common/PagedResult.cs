namespace Projet.Domain.Common;

/// <summary>Une page de résultats ; le total vient d'un <c>COUNT</c> de la même unité de travail, cohérent avec elle.</summary>
public sealed class PagedResult<T>
{
    public PagedResult()
    {
    }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public IReadOnlyList<T> Items { get; set; } = [];

    public int TotalCount { get; set; }

    /// <summary>À partir de 1.</summary>
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => PageNumber > 1;

    public bool HasNext => PageNumber < TotalPages;

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 20) =>
        new([], 0, pageNumber, pageSize);

    /// <summary>Même page, éléments convertis : les champs de pagination ne se recopient pas à la main.</summary>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        new(Items.Select(map).ToList(), TotalCount, PageNumber, PageSize);
}
