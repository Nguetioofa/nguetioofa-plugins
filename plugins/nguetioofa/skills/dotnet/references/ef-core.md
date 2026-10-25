# EF Core : entités, configurations, dépôts, transactions

Exemple suivi d'un bout à l'autre : la table `UserDevices` (les appareils d'un compte), son
entité, sa configuration et son dépôt.

---

## Entités (`Domain/Entities/`)

- En anglais, au **singulier**, `sealed`, **sans aucun attribut EF ni dépendance**.
- Toute entité hérite de `Entity<TKey> where TKey : struct`, `TKey` = type de sa colonne
  `Id` (`short`, `int`, `long`).
- ⚠ **`Entity<TKey>` ne porte que `Id`, et n'en portera jamais davantage** : pas d'égalité
  par `Id` (deux entités neuves auraient le même `0` et deviendraient « égales »), pas de
  colonnes d'audit (elles varient d'une table à l'autre), pas d'autre classe de base.
- Propriétés **dans l'ordre des colonnes du script**, groupées par les mêmes commentaires
  de section (`// ---- confiance et révocation`).
- Marqueurs dans `Domain/Common/`, dont les membres vont **toujours ensemble** :

  | Interface | Membres | Effet |
  |---|---|---|
  | `IHasTimestamps` | `CreatedAt`, `UpdatedAt` | l'intercepteur pose les dates |
  | `IAuditable : IHasTimestamps` | + `CreatedBy`, `UpdatedBy` | … et l'auteur |
  | `ISoftDeletable` | `IsDeleted`, `DeletedAt`, `DeletedBy`, `DeletionReason` | suppression logique, filtre `SoftDelete` |
  | `ITenantScoped` *(multi-tenant)* | `OrganizationId` (ou `TenantId`) | filtre de tenant |
  | `IUnitScoped : ITenantScoped` *(multi-tenant)* | + `UnitId`, l'unité (école, agence…) | filtre sur l'unité de la requête |

  Une table qui n'a qu'une partie des colonnes n'implémente pas l'interface.
- **Valeurs par défaut du schéma recopiées en C#** (`= "fr"`, `= DeviceStatus.Active`,
  `= true`) : un objet neuf ne part jamais avec un statut à `0` ou un booléen contraire au
  schéma.
- Chaînes obligatoires : `= null!` (ou `= string.Empty` pour une requête). Dates en
  `DateTime`, **toujours UTC**.
- Navigations : une référence par FK ; une collection seulement là où elle sert une
  lecture (`= []`). **Pas de navigation vers les colonnes `…By`**.
- Commentaires XML : le rôle de la table, ses règles non évidentes ; une propriété n'est
  commentée que si son nom ne suffit pas.

## Énumérations (`Domain/Enums/`)

```csharp
/// <summary>État d'un appareil.</summary>
/// <remarks>
/// Colonne <c>UserDevices.Status</c>. … Valeurs stockées en base : ne jamais les renuméroter.
/// </remarks>
public enum DeviceStatus : short
{
    Active = 1,
    Revoked = 2,
    Blocked = 3,
}
```

- Type `short` (le `SMALLINT`), valeurs explicites recopiées du script, **aucun attribut**
  (ni `[Display]`, ni code texte explicite).
- ⚠ Le **nom de la colonne va dans `<remarks>`, jamais dans `<summary>`** : le résumé est
  publié dans le document OpenAPI, donc lu par le frontend, qui ne doit rien savoir des
  tables.
- Libellé de chaque valeur : clé `Enum.<Type>.<CODE>` en fr et en en (voir `socle-api.md`).

---

## Le contexte : ses `DbSet`, rien d'autre

```csharp
public sealed partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<User> Users => Set<User>();
    // … une propriété calculée par table, par ordre alphabétique

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    // Le script se passe volontairement d'index sur certaines FK : EF n'en invente aucun.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
}
```

Seule exception admise : le fichier partiel **`<Contexte>.Filters.cs`**, qui porte les
filtres globaux (voir `multi-tenant.md`).

## Une configuration par entité

`Infrastructure/Persistence/Configurations/<Entité>Configuration.cs`, `sealed`,
`IEntityTypeConfiguration<T>`, **trois sections commentées** : colonnes, index, relations.
Aucune configuration fluente ailleurs. Elle **recopie exactement le script** :

```csharp
builder.ToTable("UserDevices");
builder.HasKey(e => e.Id).HasName("PK_UserDevices");

// ---------------------------------------------------------- colonnes
builder.Property(e => e.Id).UseIdentityAlwaysColumn();
builder.Property(e => e.PublicId).HasDefaultValueSql("gen_random_uuid()");
builder.Property(e => e.FirstSeenAt).HasDefaultValueSql("now()");
builder.Property(e => e.Fingerprint).HasMaxLength(128).IsRequired();
builder.HasXminConcurrencyToken();

// ------------------------------------------------------------- index
builder.HasIndex(e => e.PublicId, "UQ_UserDevices_PublicId").IsUnique();
builder.HasIndex(e => new { e.UserId, e.LastSeenAt }, "IX_UserDevices_LastSeen").IsDescending(false, true);

// --------------------------------------------------------- relations
builder.HasOne(e => e.User).WithMany(e => e.Devices)
    .HasForeignKey(e => e.UserId)
    .HasConstraintName("FK_UserDevices_Users")
    .OnDelete(DeleteBehavior.Cascade);
```

- Noms de clé, contraintes et index **du script** (`HasName`, `HasIndex(…, "UQ_…")`,
  `HasConstraintName`, `HasFilter` pour un index filtré). Un écart compile et ne se voit
  qu'à l'exécution.
- `HasMaxLength` partout où le script borne, `IsFixedLength()` pour un `CHAR`,
  `HasPrecision(12, 2)` pour un `NUMERIC`, **`HasColumnType("jsonb")`** pour un JSONB
  (sinon la chaîne part en `text` et PostgreSQL refuse).
- ⚠ **`HasDefaultValueSql` seulement pour ce que la base génère** (`gen_random_uuid()`,
  `now()`). Les autres défauts (`'fr'`, `Status = 1`, `IsActive = true`) sont posés par
  l'entité : déclarés en EF, il omettrait toute valeur égale au défaut du type, et un
  `IsActive` à `true` par défaut ne pourrait plus être enregistré à `false`.
- Relations déclarées **du côté qui porte la FK**. `Cascade` là où le script dit
  `ON DELETE CASCADE`, **`NoAction`** partout ailleurs (pas `ClientSetNull`).
- **Pas de convention de nommage** (ni snake_case) : colonnes = propriétés.
- Une fois les configurations écrites : comparer au catalogue réel (0 écart), et savoir
  que tant qu'une configuration manque, EF découvre l'entité par les navigations et la
  mappe par convention (`Order` au lieu de `Orders`).
- **Base et modèle EF strictement identiques** : une route de dev (`GET /api/dev/checks/model-schema`)
  compare les deux sens (tables, colonnes et leur ordre, clés, index, FK, défauts des entités
  neuves), en lecture seule, après chaque migration. **Une contrainte que le modèle ne sait pas
  décrire ne s'ajoute pas à la base.**
- **Ancres des clés composites** : toute clé unique visée par une FK composite
  (`UNIQUE (OrganizationId, Id)`) se déclare `HasAlternateKey(…).HasName("UQ_…")`, sinon EF en
  invente une nommée `AK_…`.
- **`UNIQUE NULLS NOT DISTINCT`** (PostgreSQL 15+) : un index unique avec
  `.AreNullsDistinct(false)`, jamais une clé alternative.
- ⚠ **Une colonne d'identité qui entre dans une clé étrangère composite se déclare
  `ValueGeneratedOnAdd()`** : sinon l'`INSERT` envoie `Id = 0` (`428C9`). Ni la comparaison au
  catalogue ni le fournisseur en mémoire ne le voient : un test le garde.
- `ON DELETE SET NULL ("Colonne")` sur une clé composite : `SetNull` (EF ne remet à `null` que la
  propriété nullable, comme la base).
- **Un `byte[]` suivi** (empreinte, chiffré) se remplace par un **tableau neuf** : EF compare la
  référence, une modification en place n'est pas vue.

## Concurrence optimiste

- **PostgreSQL** : colonne système `xmin`, rien à créer. Extension
  `HasXminConcurrencyToken()` = propriété fantôme `uint` nommée `Version`, `IsRowVersion()`,
  appelée par **toutes** les configurations. **SQL Server** : `rowversion`.
- La version **sort dans le JSON** des ressources modifiables (champ `version`, `int64`
  dans l'OpenAPI) et **y revient** à l'enregistrement : le dépôt la replace comme valeur
  lue (`Entry(e).Property("Version").OriginalValue = version`) avant d'écrire. Changement
  entre-temps → `DbUpdateConcurrencyException` → `409 CONCURRENCY_CONFLICT`, rien d'écrasé.
- Pour transporter la version sans la mettre dans le domaine : `Versioned<T>(T Entity, uint
  Version)` rendu par `FindWithVersionAsync`, et `FindForUpdateAsync(id, version?)`.
- Supprimer une ligne protégée par `xmin` exige aussi sa version — et, si les colonnes
  d'espace sont des jetons de concurrence (`multi-tenant.md`), `OrganizationId` et `UnitId`
  posés comme **valeurs d'origine**.
- Une version dans l'adresse (un `DELETE …?version=`) se déclare `[BindRequired]` ; absente, la
  règle du projet tranche (refus `400` ou `409`) et se documente dans l'OpenAPI.

## Intercepteurs, jamais le contexte

- `AuditInterceptor` (`SaveChangesInterceptor`) : `DetectChanges` explicite en tête ;
  création → `CreatedAt`/`CreatedBy` ; modification → `UpdatedAt`/`UpdatedBy`, les deux
  `Created…` rendus non modifiables ; `Remove()` d'un `ISoftDeletable` → mise à jour de
  `IsDeleted`, `DeletedAt`, `DeletedBy` **sans réécrire les autres colonnes** (une entité
  attachée par son seul `Id` garde ses données). Toute valeur posée par un service sur ces
  colonnes est ignorée. L'auteur vient d'`ICurrentUser` (`null` = le système).
- ⚠ `ExecuteUpdate` / `ExecuteDelete` / SQL brut **passent hors de l'intercepteur et hors
  des filtres** : y poser l'audit et le filtre de tenant à la main ; un `ExecuteDelete`
  supprime vraiment.
- Pas de `EnableRetryOnFailure` : il interdit les transactions ouvertes par le code.

---

## Dépôts

**Pas de dépôt générique.** Un dépôt par entité, avec **exactement les méthodes que sa
table justifie**. Contrat `Domain/Repositories/I<Entité>Repository.cs` (commentaire XML par
méthode : ce qu'elle rend, si c'est suivi ou non, ce que `null` veut dire),
implémentation `sealed` dans `Infrastructure/Persistence/Repositories/`, enregistrée **un
par un** dans le module Autofac de persistance.

```csharp
public sealed class UserDeviceRepository(AppDbContext db) : IUserDeviceRepository
{
    public Task<UserDevice?> FindByPublicIdAsync(int userId, Guid publicId, CancellationToken cancellationToken = default) =>
        db.UserDevices.FirstOrDefaultAsync(d => d.UserId == userId && d.PublicId == publicId, cancellationToken);

    public async Task<IReadOnlyList<UserDevice>> ListAsync(int userId, CancellationToken cancellationToken = default) =>
        await db.UserDevices.AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync(cancellationToken);

    public void Add(UserDevice device) => db.UserDevices.Add(device);
}
```

- Lecture pour affichage : `AsNoTracking()`. Lecture pour modification : suivie.
- **Chercher « à moi » dans la même requête** (`UserId == userId && PublicId == publicId`) :
  la donnée d'un autre et la donnée inexistante se confondent (404 dans les deux cas).
- Listes : projection sur `IQueryable` (`ProjectToListItem()`), seules les colonnes utiles.
- **Écritures concurrentes** (compteur d'essais, verrouillage, rotation d'un jeton) :
  `ExecuteUpdateAsync` **conditionnel** qui rend le nombre de lignes touchées — un
  compare-and-swap (`WHERE "RefreshTokenHash" = @lu`). Jamais lire-modifier-écrire pour un
  compteur disputé.
- Un dépôt qui ne doit **pas** tourner dans une transaction (essais d'un code comptés même
  si l'opération échoue) le vérifie et lève une exception si on l'y appelle.

## Unité de travail

- Les dépôts **enregistrent immédiatement**. Une opération qui touche plusieurs tables
  passe par `IUnitOfWork.ExecuteInTransactionAsync(async ct => { … return Result…; }, ct)` :
  - validée si l'opération rend un **succès** ; annulée si elle rend une **erreur** ou lève
    une exception — et `ChangeTracker.Clear()` oublie les modifications en mémoire ;
  - un `SaveChanges` final avant la validation ;
  - appelée dans une transaction déjà ouverte, elle s'y joint et laisse décider
    l'englobante.
- Une règle refusée à mi-chemin **rend une erreur, ne lève pas** : c'est ce qui annule les
  écritures déjà faites.
- ⚠ `Result` est une **struct** : `Result.Failure(error)` dans une lambda qui rend
  `Task<Result>`, pas `return error;` si l'inférence hésite.
- Une trace de sécurité qui doit rester **quoi qu'il arrive** (échec de connexion, jeton
  rejoué) s'écrit **hors** transaction.
