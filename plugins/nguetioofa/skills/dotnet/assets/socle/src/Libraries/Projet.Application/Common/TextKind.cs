namespace Projet.Application.Common;

/// <summary>Ce qu'est un texte reçu : d'une ligne par défaut, long, ou mot de passe (jamais nettoyé).</summary>
public enum TextKind
{
    Line,
    Multiline,
    Password,
}
