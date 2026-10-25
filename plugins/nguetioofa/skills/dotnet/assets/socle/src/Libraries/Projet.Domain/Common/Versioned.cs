namespace Projet.Domain.Common;

/// <summary>Une entité et sa version lue ; rendue périmée, la modification échoue en 409 <c>CONCURRENCY_CONFLICT</c>.</summary>
public sealed record Versioned<T>(T Entity, uint Version)
    where T : class;
