namespace Projet.Application.Common;

/// <summary>Un texte long d'une demande : ses sauts de ligne sont gardés ; sans cette marque, ils le font refuser.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class MultilineTextAttribute : Attribute;
