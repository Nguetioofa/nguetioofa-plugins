using System.Globalization;
using System.Resources;

namespace Projet.Application.Common;

/// <summary>Les textes dans une langue choisie par le code (destinataire, contrôle du démarrage) ; <c>GetStrictSet</c> sans repli.</summary>
public static class SharedResources
{
    public const string BaseName = "Projet.Application.Resources.Common.SharedResource";

    public static ResourceManager Manager { get; } = new(BaseName, typeof(SharedResource).Assembly);

    public static ResourceSet? GetStrictSet(CultureInfo culture, CultureInfo neutralCulture) =>
        Manager.GetResourceSet(
            culture.Equals(neutralCulture) ? CultureInfo.InvariantCulture : culture,
            createIfNotExists: true,
            tryParents: false);
}
