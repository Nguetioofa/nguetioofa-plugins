using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using Projet.Application.Common;

namespace Projet.API.Json;

/// <summary>Écrit et lit les énumérations en texte, dans la seule écriture publiée (<c>PENDING_PAYMENT</c>).</summary>
/// <remarks>Pas <c>JsonStringEnumConverter</c> : il accepte <c>"trial"</c>, <c>" TRIAL"</c>, et fait de <c>"TRIAL, ACTIVE"</c>
/// une troisième valeur (1 | 2), sans erreur. Ici, toute autre écriture, entiers compris, est un 400 sous le champ.</remarks>
public sealed class StrictEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Activator.CreateInstance(typeof(StrictEnumConverter<>).MakeGenericType(typeToConvert))!;

    private sealed class StrictEnumConverter<TEnum> : JsonConverter<TEnum>
        where TEnum : struct, Enum
    {
        private static readonly FrozenDictionary<string, TEnum> ValuesByName = Enum.GetNames<TEnum>()
            .ToFrozenDictionary(EnumCodes.ToCode, Enum.Parse<TEnum>, StringComparer.Ordinal);

        // Deux membres de même valeur (alias) : le premier déclaré donne son nom.
        private static readonly FrozenDictionary<TEnum, string> NamesByValue = Enum.GetNames<TEnum>()
            .DistinctBy(Enum.Parse<TEnum>)
            .ToFrozenDictionary(Enum.Parse<TEnum>, EnumCodes.ToCode);

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType == JsonTokenType.String
                ? Parse(reader.GetString())
                : throw new JsonException();

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
            writer.WriteStringValue(NameOf(value));

        public override TEnum ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Parse(reader.GetString());

        public override void WriteAsPropertyName(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
            writer.WritePropertyName(NameOf(value));

        private static TEnum Parse(string? name) =>
            name is not null && ValuesByName.TryGetValue(name, out var value)
                ? value
                : throw new JsonException();

        // Une valeur hors de l'énumération est un bug du code : la faire échouer plutôt que la publier en chiffre.
        private static string NameOf(TEnum value) =>
            NamesByValue.TryGetValue(value, out var name)
                ? name
                : throw new InvalidOperationException(
                    $"La valeur {value} n'appartient pas à l'énumération {typeof(TEnum).Name} : elle n'a aucune écriture publiée.");
    }
}
