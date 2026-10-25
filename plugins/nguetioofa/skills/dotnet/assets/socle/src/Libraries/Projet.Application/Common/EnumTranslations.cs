using System.Globalization;
using Projet.Domain.Common;

namespace Projet.Application.Common;

/// <summary>Vérifie au démarrage que chaque valeur des énumérations du domaine a son libellé dans chaque langue.</summary>
/// <remarks>Lu sans repli : sinon une clé absente en anglais serait trouvée en français, et le contrôle ne verrait rien.</remarks>
public static class EnumTranslations
{
    private const string EnumsNamespace = "Projet.Domain.Enums";

    public static IReadOnlyList<string> FindMissing(IEnumerable<CultureInfo> cultures, CultureInfo neutralCulture)
    {
        var keys = typeof(Entity<>).Assembly.GetTypes()
            .Where(type => type.IsEnum && type.Namespace == EnumsNamespace)
            .SelectMany(type => Enum.GetNames(type).Select(name => EnumCodes.LabelKey(type, name)))
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToList();

        var missing = new List<string>();
        foreach (var culture in cultures)
        {
            var resources = SharedResources.GetStrictSet(culture, neutralCulture);
            missing.AddRange(keys
                .Where(key => string.IsNullOrWhiteSpace(resources?.GetString(key)))
                .Select(key => $"{culture.Name} : {key}"));
        }

        return missing;
    }

    /// <exception cref="InvalidOperationException">Au moins un libellé manque.</exception>
    public static void EnsureComplete(IEnumerable<CultureInfo> cultures, CultureInfo neutralCulture)
    {
        var missing = FindMissing(cultures, neutralCulture);
        if (missing.Count == 0)
            return;

        throw new InvalidOperationException(
            $"{missing.Count} libellé(s) d'énumération manquant(s). Chaque valeur de Domain/Enums a sa clé " +
            "Enum.<Type>.<CODE> dans SharedResource.resx (français) et SharedResource.en.resx (anglais) :" +
            $"{Environment.NewLine}{string.Join(Environment.NewLine, missing)}");
    }
}
