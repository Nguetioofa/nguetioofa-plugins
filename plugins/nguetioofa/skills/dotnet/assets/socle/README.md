# Gabarit du socle d'une API ASP.NET Core

Une solution .NET 10 minimale qui compile et démarre, et qui porte le socle décrit par la fiche
[`references/socle-api.md`](../../references/socle-api.md) du skill : tout ce qui entoure chaque requête,
avant la première fonctionnalité.

## Ce qu'il contient

| Couche | Contenu |
|---|---|
| `Projet.Domain` (aucune référence) | `Result` / `Result<T>` et leurs combinateurs asynchrones, `Error`, `ErrorCodes`, `PagedResult<T>`, `Versioned<T>`, `Entity<TKey>`, marqueurs `IHasTimestamps`, `IAuditable`, `ISoftDeletable`, `ITenantScoped`, `IUnitScoped` |
| `Projet.Application` | enveloppe `ApiResponse<T>`, `PagedRequest` et `PagingRules`, `ICurrentUser` / `SystemCurrentUser`, `IRequestInfo`, `UtcTime`, `EnumCodes` et le contrôle des libellés au démarrage, nettoyage des textes reçus (`TextRules`, `TextCleaner`, `RequestTexts`, `[MultilineText]`, `[PasswordText]`), plafond des erreurs, `SharedResource.resx` (fr, neutre) + `.en.resx`, `ApplicationModule` (validateurs par balayage) |
| `Projet.Infrastructure` | `InfrastructureModule` Autofac, vide |
| `Projet.API` | `Program.cs` qui n'assemble que des extensions ; corps de requête borné (413 dans l'enveloppe) ; localisation par `Accept-Language` ; erreurs uniformes (`ApiControllerBase`, fabrique du `ModelState`, `FluentValidationFilter`, `ExceptionHandlingMiddleware`, pages de statut) ; convertisseur JSON strict des énumérations ; liaison stricte de l'adresse (énumération, valeur répétée) ; `RequestTextFilter` ; Swagger en développement (`CommonResponsesOperationFilter`, `ResponseSchemaFilter`) ; `ICurrentUser` / `IRequestInfo` HTTP |

`Directory.Build.props` (nullabilité en erreur, documentation XML), `Directory.Packages.props`
(versions centralisées : Autofac, FluentValidation, Swashbuckle), `Projet.slnx`, `.gitignore`.

## L'adopter

1. Copier le dossier `src/` dans `backend/` du nouveau projet.
2. Renommer `Projet` partout, dans les noms de fichiers et de dossiers **et** dans leur contenu
   (rechercher-remplacer sensible à la casse : `Projet.` → `MonProduit.`). Les chaînes
   `Projet.Domain.Enums`, `Projet.Application.DTOs` et `Projet.Application.Resources.Common.SharedResource`
   suivent ce renommage ; le titre `Projet API` de Swagger aussi.
3. `dotnet build` (zéro avertissement attendu), puis `dotnet run --project src/Presentation/<Nom>.API`
   et `GET /api/nexiste-pas` : un 404 `ROUTE_NOT_FOUND` dans l'enveloppe.
4. Choisir le port dans `Properties/launchSettings.json` et la version de l'API publiée dans
   `Extensions/SwaggerExtensions.cs`.

## Ce qu'il ne contient pas

Chaque sujet a sa fiche dans le skill ; il s'ajoute par ses propres extensions et modules.

- Persistance EF Core, intercepteurs, `IUnitOfWork`, scripts SQL : `ef-core.md`, `base-de-donnees.md`.
  Avec EF, ajouter au gestionnaire d'exceptions le cas `DbUpdateConcurrencyException` → 409
  `CONCURRENCY_CONFLICT` (clé `Error.Concurrency` en fr et en en).
- Authentification, cookies, limite de débit : `authentification.md`. Elle pose `CurrentSession` dans
  `HttpContext.Features` et passe `MapApiControllers` à `MapControllers().RequireAuthorization()` ;
  sans elle, toute requête est anonyme.
- Tenancy et permissions : `multi-tenant.md`.
- Paramètres en base, audit, notifications, normaliseur d'identifiants, `GET /api/lookups` :
  `socle-api.md`, section « Catalogues typés ».
- Proxy inverse (`UseForwardedHeaders`), CORS, sonde de santé : `production.md` ; tests xUnit :
  `verification-et-tests.md`.
