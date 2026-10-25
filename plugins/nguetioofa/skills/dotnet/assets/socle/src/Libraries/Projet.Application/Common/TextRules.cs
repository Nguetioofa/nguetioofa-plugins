using System.Globalization;
using System.Text;

namespace Projet.Application.Common;

/// <summary>Seul endroit qui dit ce que l'API retire d'un texte reçu en silence, puis ce qu'elle refuse de ce qui reste.</summary>
/// <remarks>
/// Retirés : les blancs des bords et les caractères de format invisibles (<c>Cf</c>), pour que deux textes qui se
/// lisent pareil soient le même. Refusés : caractère de contrôle (le nul ferait échouer l'écriture en base), saut
/// de ligne dans un texte d'une ligne, moitié de caractère. Ce qui est jugé est ce qui sera gardé.
/// </remarks>
public static class TextRules
{
    public static bool TryClean(TextKind kind, string text, out string clean)
    {
        clean = Clean(kind, text);
        return IsAdmitted(kind, clean);
    }

    public static bool Accepts(TextKind kind, string text) => TryClean(kind, text, out _);

    /// <summary>Mot de passe : rien. Texte long : en plus, fins de ligne en <c>\n</c>, blancs de fin de ligne retirés.</summary>
    public static string Clean(TextKind kind, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return kind switch
        {
            TextKind.Password => text,
            TextKind.Multiline => WithCleanLines(WithoutFormat(text)),
            _ => WithoutFormat(text).Trim(),
        };
    }

    /// <summary>Aucun demi-codet isolé : ni la base ni la normalisation ne savent le lire.</summary>
    public static bool IsWellFormed(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        for (var i = 0; i < text.Length; i++)
        {
            if (!char.IsSurrogate(text[i]))
                continue;
            if (!char.IsHighSurrogate(text[i]) || i + 1 >= text.Length || !char.IsLowSurrogate(text[i + 1]))
                return false;
            i++;
        }

        return true;
    }

    /// <summary>Un code tel qu'il sera gardé, jugé sans nettoyage : ASCII imprimable, sans espace (<c>A1-EN</c>).</summary>
    public static bool IsCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        foreach (var c in code)
        {
            if (c is < '!' or > '~')
                return false;
        }

        return true;
    }

    private static bool IsAdmitted(TextKind kind, string text)
    {
        if (kind == TextKind.Password)
            return IsWellFormed(text);

        var multiline = kind == TextKind.Multiline;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsSurrogate(c))
            {
                if (!char.IsHighSurrogate(c) || i + 1 >= text.Length || !char.IsLowSurrogate(text[i + 1]))
                    return false;
            }

            switch (CharUnicodeInfo.GetUnicodeCategory(text, i))
            {
                case UnicodeCategory.Control:
                    if (!multiline || c is not ('\n' or '\t'))
                        return false;
                    break;
                case UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator:
                    if (!multiline)
                        return false;
                    break;
                // Aucun ne reste après nettoyage ; refusé pour que le jugement ne dépende pas de l'appelant.
                case UnicodeCategory.Format:
                    return false;
            }

            if (char.IsHighSurrogate(c))
                i++;
        }

        return true;
    }

    private static string WithoutFormat(string text)
    {
        var first = IndexOfFormat(text, 0);
        if (first < 0)
            return text;

        var kept = new StringBuilder(text.Length);
        var start = 0;
        for (var i = first; i >= 0; i = IndexOfFormat(text, start))
        {
            kept.Append(text, start, i - start);
            start = i + (char.IsHighSurrogate(text[i]) ? 2 : 1);
        }

        return kept.Append(text, start, text.Length - start).ToString();
    }

    private static int IndexOfFormat(string text, int from)
    {
        for (var i = from; i < text.Length; i++)
        {
            // Lu par caractère entier : une paire de demi-codets est un seul caractère.
            var paired = char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]);
            if (CharUnicodeInfo.GetUnicodeCategory(text, i) == UnicodeCategory.Format)
                return i;
            if (paired)
                i++;
        }

        return -1;
    }

    private static string WithCleanLines(string text)
    {
        if (text.Contains('\r'))
            text = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

        text = text.Trim();
        if (!text.Contains('\n'))
            return text;

        var lines = text.Split('\n');
        var changed = false;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd();
            changed |= line.Length != lines[i].Length;
            lines[i] = line;
        }

        return changed ? string.Join('\n', lines) : text;
    }
}
