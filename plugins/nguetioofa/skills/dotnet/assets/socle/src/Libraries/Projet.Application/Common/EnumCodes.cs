using System.Text.Json;

namespace Projet.Application.Common;

/// <summary>Seul calcul du code publié (<c>PENDING_PAYMENT</c>) et de la clé du libellé, partagé par le JSON et les libellés.</summary>
public static class EnumCodes
{
    public static string ToCode(string memberName) =>
        JsonNamingPolicy.SnakeCaseUpper.ConvertName(memberName);

    public static string LabelKey(Type enumType, string memberName) =>
        $"Enum.{enumType.Name}.{ToCode(memberName)}";
}
