using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Projet.API.Validation;

/// <summary>Traduit le <c>[Required]</c> implicite de MVC (un <c>null</c> explicite n'atteint pas FluentValidation) en le remplaçant, sans couper la validation implicite.</summary>
public sealed class ImplicitRequiredMessageProvider : IValidationMetadataProvider
{
    public const string MessageKey = "Validation.Required";

    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var validators = context.ValidationMetadata.ValidatorMetadata;
        for (var i = 0; i < validators.Count; i++)
        {
            if (validators[i] is RequiredAttribute { ErrorMessage: null, ErrorMessageResourceType: null } required)
                validators[i] = new RequiredAttribute { AllowEmptyStrings = required.AllowEmptyStrings, ErrorMessage = MessageKey };
        }
    }
}
