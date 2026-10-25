namespace Projet.Domain.Common;

/// <summary>Base de toute entité : sa clé seule, sans égalité par <c>Id</c> (deux entités neuves seraient « égales »).</summary>
public abstract class Entity<TKey>
    where TKey : struct
{
    public TKey Id { get; set; }
}
