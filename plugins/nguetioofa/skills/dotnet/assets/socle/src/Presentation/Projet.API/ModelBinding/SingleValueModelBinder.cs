using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Projet.API.ModelBinding;

/// <summary>Refuse en 400 une valeur répétée (<c>?id=…&amp;id=…</c>), puis laisse faire le lieur que MVC aurait choisi.</summary>
/// <remarks>MVC garderait la première : filtré sur l'une, l'écran croirait avoir demandé les deux.</remarks>
public sealed class SingleValueModelBinder(IModelBinder inner) : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (result.Length <= 1)
            return inner.BindModelAsync(bindingContext);

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);
        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            bindingContext.ModelMetadata.ModelBindingMessageProvider.AttemptedValueIsInvalidAccessor(result.ToString(), bindingContext.ModelName));
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}
