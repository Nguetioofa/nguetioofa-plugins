using System.Diagnostics.CodeAnalysis;

namespace Projet.Application.Common;

/// <summary>La forme sous laquelle un service garde un texte saisi (<see cref="TextRules"/>) ; <c>null</c> pour un texte vide.</summary>
public static class TextCleaner
{
    public static string? Clean(string? text) =>
        string.Join(' ', TextRules.Clean(TextKind.Line, text ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) is { Length: > 0 } clean
            ? clean
            : null;

    public static string? CleanMultiline(string? text) =>
        TextRules.Clean(TextKind.Multiline, text ?? string.Empty) is { Length: > 0 } clean ? clean : null;

    /// <summary>Jamais coupé entre deux demi-codets : la base refuserait la moitié de caractère.</summary>
    [return: NotNullIfNotNull(nameof(text))]
    public static string? Truncate(string? text, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        if (text is null || text.Length <= maxLength)
            return text;

        return text[..(maxLength > 0 && char.IsHighSurrogate(text[maxLength - 1]) ? maxLength - 1 : maxLength)];
    }
}
