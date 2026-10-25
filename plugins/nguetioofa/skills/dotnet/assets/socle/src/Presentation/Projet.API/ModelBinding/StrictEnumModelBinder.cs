using System.Collections.Frozen;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Projet.Application.Common;

namespace Projet.API.ModelBinding;

/// <summary>Lit une énumération de l'adresse (<c>?status=SUSPENDED</c>) dans la seule écriture publiée ; répétée, refusée.</summary>
/// <remarks>Le convertisseur du framework accepte <c>suspended</c>, <c>2</c> ou <c>Active, Suspended</c>, et refuse <c>PENDING_PAYMENT</c>.</remarks>
public sealed class StrictEnumModelBinder(Type enumType) : IModelBinder
{
    private readonly FrozenDictionary<string, object> _values = Enum.GetNames(enumType)
        .ToFrozenDictionary(EnumCodes.ToCode, name => Enum.Parse(enumType, name), StringComparer.Ordinal);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (result == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);

        if (result.Length > 1)
            return Fail(bindingContext, result.ToString());

        var raw = result.FirstValue;

        // Présent mais vide (?status=) : pas de filtre, comme s'il était absent.
        if (string.IsNullOrEmpty(raw) && bindingContext.ModelMetadata.IsReferenceOrNullableType)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (raw is not null && _values.TryGetValue(raw, out var value))
        {
            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }

        return Fail(bindingContext, raw ?? string.Empty);
    }

    private static Task Fail(ModelBindingContext bindingContext, string attempted)
    {
        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            bindingContext.ModelMetadata.ModelBindingMessageProvider.AttemptedValueIsInvalidAccessor(attempted, bindingContext.ModelName));
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}
