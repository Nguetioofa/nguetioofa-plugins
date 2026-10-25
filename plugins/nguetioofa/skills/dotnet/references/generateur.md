# Projets produits par ProjectGenerator

L'outil maison de l'utilisateur (son dépôt porte un `README.md` et un `docs/PLAN.md` ; le
`CLAUDE.md` du projet généré dit où il est). Il lit la base (SQL Server ; **il ne lit pas
PostgreSQL**) ou un document OpenAPI, et produit la quasi-totalité du CRUD : entités,
configurations EF, dépôts, DTO, requêtes, mappings, services, validateurs, contrôleurs,
modules Autofac, ressources.

**Reconnaître un projet généré** : `src/Presentation/<Projet>.API/projectgen.json`
(+ `projectgen.snapshot.json`), fichiers `*.g.cs`, jumeaux `*.Custom.cs`.

La philosophie, la base de données (même conventions, voir `base-de-donnees.md`) et les
conventions de code sont **les mêmes** qu'à la main : c'est le générateur qui les applique.

---

## Ce qui se régénère, et ce qui t'appartient

| Marque | Sort à la régénération | Exemples |
|---|---|---|
| `*.g.cs` | **réécrit sans condition** | `Result.g.cs`, `ApiResponse.g.cs`, `PermissionCatalog.g.cs`, `*Validators.g.cs`, filtres |
| Fichier généré sans suffixe | **réécrit**, sauf s'il a été modifié : il passe alors « en conflit » et reste tel quel | entités, DTO, services, dépôts, configurations, contrôleurs |
| `*.Custom.cs` | **écrit une fois, jamais retouché** | `VehicleService.Custom.cs`, `Vehicle.Custom.cs`, `VehicleRequestValidators.Custom.cs` |
| Écrit une fois | posé à la première génération, ensuite à toi | `Program.cs`, `appsettings.json`, `PermissionProvider.cs` |
| Zones `// <generated:x>` | seul l'intérieur du bloc est réécrit | modules Autofac dans `Program.cs`, liste des permissions |

**Le code métier va dans le `.Custom.cs`.** Chaque service généré appelle des méthodes
partielles — `ValidateBeforeCreateAsync`, `ValidateBeforeUpdateAsync`, `AfterCreateAsync`…
— dont le corps t'attend dans le fichier jumeau. Une correction faite dans un fichier
généré **disparaît à la prochaine régénération, sans avertissement** (ou le met en conflit).

## Où trouver quoi, pour une entité `Vehicle`

| Quoi | Où |
|---|---|
| Entité / tes ajouts | `Domain/Entities/Vehicle.cs` / `Vehicle.Custom.cs` |
| Contrat de dépôt | `Domain/Repositories/IVehicleRepository.cs` |
| Mapping EF | `Infrastructure/Persistence/Configurations/VehicleConfiguration.cs` |
| Dépôt | `Infrastructure/Persistence/Repositories/VehicleRepository.cs` |
| DTO (`VehicleDto`, `VehicleListItem`, `VehicleDetail`) | `Application/DTOs/VehicleDto.cs` |
| Requêtes (`Create…`, `Update…`, `Search…`) | `Application/Requests/VehicleRequests.cs` |
| Conversions | `Application/Mapping/VehicleMappingExtensions.cs` |
| Service | `Application/Services/VehicleService.cs` + `.Custom.cs` |
| Validateurs | `Application/Validation/VehicleRequestValidators.g.cs` + `.Custom.cs` |
| Contrôleur | `API/Controllers/VehicleController.cs` |

(Dans un projet généré, plusieurs types par fichier sont admis là où le générateur les
regroupe ; ne pas les éclater à la main.)

## Régénérer

- La configuration (`projectgen.json`) porte les entités, leurs endpoints, les colonnes
  sensibles et l'empreinte SHA-256 de chaque fichier produit. **Jamais de chaîne de
  connexion** dedans : seulement `ConnectionStrings:Default`.
- Quand le schéma change : **d'abord le script SQL** (schéma + migration datée), appliqué
  sur la base ; puis dans l'interface du générateur : Configuration → Base (relire) → Revue
  (entités, propriétés sensibles, recherche, endpoints) → Génération (voir le diff,
  **arbitrer les conflits fichier par fichier**).
- **Ne jamais écraser en bloc** : c'est ainsi qu'on perd une règle métier ou une
  chaîne de connexion.
- Après chaque régénération : `dotnet build`, puis **`ModeleEfTests`** (construit et valide
  le modèle EF sans base — un mapping faux compile et ne se voit qu'en 500 sur toutes les
  routes), puis vérification à l'exécution.
- L'IA ne lance pas le générateur à la place de l'utilisateur sans le lui demander ; elle
  prépare le script SQL et dit ce qu'il faut régénérer.

## Conventions propres au code généré

- **Colonnes sensibles** (`PasswordHash`, `TwoFactorSecret`, `CodeHash`,
  `RefreshTokenHash`, `TokenHash`…) exclues des DTO et de la recherche libre (sinon on
  devine leur contenu en sondant un filtre). Faux positifs (`PasswordChangedAt`) libérés
  par `notSensitiveProperties` dans `projectgen.json`, jamais en modifiant le DTO.
- **Permissions** : `[HasPermission(UserPermission.VEHICLE_VIEW)]` sur chaque action,
  filtre global `PermissionFilter` ; le **nom** du membre est stocké et comparé, jamais sa
  valeur numérique.
- Un nom d'entité qui coïncide avec le DTO d'une autre casse la compilation : le
  générateur choisit un autre nom (`ReservationDetailView`) — ne pas le renommer.
- Avertissements EF `10622` (filtre de suppression logique sur l'extrémité requise d'une
  relation) : attendus tant que la décision métier n'est pas prise ; ne pas les « chasser ».
- `Sensitive data logging` activé en développement : à couper avant tout déploiement.

## Défauts connus du modèle généré — ne pas les recopier

Prouvés à l'exécution sur un projet qui avait repris ce socle ; à signaler si l'on
retravaille un projet généré ou les templates du générateur :

1. `UseApiExceptionHandling()` **avant** `UseRequestLocalization()` → un 500 d'une requête
   `fr` sort dans la langue de la machine. Correct : la localisation d'abord.
2. `context.Response.Clear()` dans le middleware d'exceptions → CORS et `Content-Language`
   effacés, le navigateur ne peut plus lire l'erreur. Correct : reposer le statut et
   `ContentLength = null`, sans `Clear`.
3. `TraceId` incohérent (`Activity.Current?.Id` d'un côté, `TraceIdentifier` de l'autre).
   Correct : une seule source, `HttpContext.GetTraceId()`.
4. Middleware qui sérialise avec ses propres `JsonSerializerOptions` (accents échappés,
   options MVC ignorées). Correct : un `ApiResponseWriter` qui reprend les options de MVC.
5. Isolation multi-tenant **non implémentée** (aucun filtre de tenant) et **`Id` exposé**
   dans les routes et DTO au lieu de `PublicId` : à reprendre à la main avant toute
   ouverture.
