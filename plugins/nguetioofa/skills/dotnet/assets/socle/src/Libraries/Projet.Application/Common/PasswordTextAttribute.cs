namespace Projet.Application.Common;

/// <summary>Un mot de passe : jamais nettoyé, pour rester celui qui a été choisi ; seul un texte mal formé est refusé.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PasswordTextAttribute : Attribute;
