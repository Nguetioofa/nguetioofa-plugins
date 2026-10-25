using FluentValidation;
using Microsoft.Extensions.Localization;
using Projet.Application.Common;

namespace Projet.Application.Validation.Common;

/// <summary>Règles d'une liste paginée ; la taille de page n'en a pas, <see cref="PagedRequest"/> la ramène au défaut.</summary>
public static class PagingRules
{
    public static void Paging<T>(this AbstractValidator<T> validator, IStringLocalizer<SharedResource> localizer, params string[] sorts)
        where T : PagedRequest
    {
        ArgumentNullException.ThrowIfNull(validator);

        validator.RuleFor(r => r.PageNumber)
            .InclusiveBetween(1, PagedRequest.MaxPageNumber)
            .WithMessage(localizer["Validation.PageNumber", PagedRequest.MaxPageNumber]);

        validator.RuleFor(r => r.SearchTerm)
            .Must(term => term!.Trim().Length <= 100)
            .When(r => r.SearchTerm is not null)
            .WithMessage(localizer["Validation.MaxLength"]);

        // Un tri inconnu est refusé plutôt qu'ignoré : l'écran croirait sa liste triée.
        validator.RuleFor(r => r.SortBy)
            .Must(sort => sorts.Contains(sort, StringComparer.OrdinalIgnoreCase))
            .When(r => !string.IsNullOrEmpty(r.SortBy))
            .WithMessage(localizer["Validation.SortBy"]);
    }
}
