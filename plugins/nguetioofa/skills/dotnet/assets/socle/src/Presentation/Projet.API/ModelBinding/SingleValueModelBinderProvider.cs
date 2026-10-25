using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Projet.API.ModelBinding;

/// <summary>Enveloppe le lieur de tout paramètre d'adresse à une seule valeur ; listes, corps et en-têtes ne sont pas concernés.</summary>
public sealed class SingleValueModelBinderProvider(IList<IModelBinderProvider> providers) : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var source = context.BindingInfo.BindingSource;
        var fromAddress = source is null || source == BindingSource.Query || source == BindingSource.Path || source == BindingSource.ModelBinding;
        if (!fromAddress || context.Metadata.IsComplexType || context.Metadata.IsEnumerableType)
            return null;

        foreach (var provider in providers)
        {
            if (ReferenceEquals(provider, this))
                continue;
            if (provider.GetBinder(context) is { } inner)
                return new SingleValueModelBinder(inner);
        }

        return null;
    }
}
