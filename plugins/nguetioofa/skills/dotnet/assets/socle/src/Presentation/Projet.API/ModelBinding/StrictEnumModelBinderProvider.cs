using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Projet.API.ModelBinding;

/// <summary>Toute énumération lue hors du corps (le JSON est déjà strict) ; en tête, avant le fournisseur des types simples.</summary>
public sealed class StrictEnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        if (!type.IsEnum || context.BindingInfo.BindingSource == BindingSource.Body)
            return null;

        return new StrictEnumModelBinder(type);
    }
}
