# Le socle d'une API

Ce qu'un projet met en place avant sa première fonctionnalité, et les règles de ce qui
tourne autour de chaque requête. Cette fiche décrit le socle **en entier** : un projet neuf
le construit d'après elle, dans l'ordre des sections. Chaque détail en a été corrigé à
l'exécution ; ne pas le simplifier. Si l'utilisateur désigne un de ses projets existants comme
source, le reprendre et le réduire au besoin, en disant ce qui a changé.

---

## Solution

```
backend/
├─ CLAUDE.md                    guide de l'agent : stack, structure, règles, commandes, état
├─ docs/                        conception, plan d'implémentation
├─ scripts/db/                  schéma de référence, migrations, vérifications
└─ src/
   ├─ <Projet>.slnx
   ├─ Directory.Build.props     net10.0, nullable en ERREUR, doc XML, NoWarn 1591
   ├─ Directory.Packages.props  versions de TOUS les paquets (un .csproj n'a jamais de Version)
   ├─ Libraries/
   │  ├─ <Projet>.Domain/          aucune référence
   │  ├─ <Projet>.Application/     FrameworkReference Microsoft.AspNetCore.App
   │  └─ <Projet>.Infrastructure/
   ├─ Presentation/<Projet>.API/
   └─ Tests/<Projet>.Test/
```

- `Directory.Build.props` : `TargetFramework net10.0`, `Nullable enable`,
  `WarningsAsErrors nullable` (un avertissement de nullabilité est un bug),
  `GenerateDocumentationFile true` dans **toutes** les couches (les commentaires XML
  alimentent l'OpenAPI), `NoWarn 1591`.
- Paquets de base : EF Core + Npgsql (ou SqlServer), Autofac +
  Autofac.Extensions.DependencyInjection, FluentValidation, Swashbuckle.AspNetCore,
  JwtBearer, MailKit ; tests : xunit, Microsoft.NET.Test.Sdk. **Pas** d'ASP.NET Identity,
  **pas** d'AutoMapper, **pas** d'EFCore.NamingConventions. Commenter dans le `.props` les
  couplages de versions (« Autofac.Extensions 11 exige Autofac ≥ 9.3.1 »).
- `.gitignore` : ajouter `appsettings.Development.json` et `Controllers/Dev/`.
- `CLAUDE.md` du projet : il dit **ce qui est**, jamais ce qui s'est passé — ni date, ni numéro
  de décision, ni « fait le… », ni preuve, ni liste de ce qui a été construit (tout cela va au
  plan et dans git). Une section « Où en est le projet » en **tableau court** (chantier, état,
  lien vers le plan) ; les règles **propres** au projet, une ou deux lignes chacune, rangées
  **par thème** ; rien de ce que dit ce skill. Quand les règles d'un domaine grossissent, elles
  passent dans un skill du projet (une référence par domaine) que le `CLAUDE.md` indexe.

**Le gabarit** : `assets/socle/` (son `README.md` dit comment l'adopter) contient le socle de
requête et de réponse décrit ci-dessous, prêt à compiler — `Result`, `Error`, l'enveloppe,
les filets d'erreur, la localisation, le convertisseur strict, la liaison stricte, le
nettoyage des textes, la borne des corps de requête, le contrôleur de base. Un projet neuf le
copie et renomme `Projet` (noms de fichiers et contenu). Ce qu'il **ne contient pas** s'ajoute
d'après sa fiche :
- persistance EF, et avec elle `DbUpdateConcurrencyException` → `409 CONCURRENCY_CONFLICT` dans
  le gestionnaire d'exceptions (clé `Error.Concurrency` fr et en) → `ef-core.md` ;
- authentification, puis `MapControllers().RequireAuthorization()` et le cadenas de Swagger →
  `authentification.md` ;
- tenancy, permissions, lecture seule → `multi-tenant.md` ;
- CORS, proxy, sonde de santé, limite de débit, tâche d'entretien → `production.md` ;
- catalogues des paramètres, de l'audit et des notifications → la section « Catalogues typés »
  ci-dessous.

Le gabarit est fait pour une API ; un projet MVC ou Blazor en reprend `Domain/Common` et
`Application/Common` (voir `types-de-projets.md`).

---

## `Program.cs` n'assemble que

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseAutofacContainer();

builder.Services
    .AddXxxPersistence(builder.Configuration, builder.Environment)   // Infrastructure, vérifié au démarrage
    .AddXxxNotifications(builder.Configuration, builder.Environment)
    .AddXxxSecurity(builder.Configuration)
    .AddApiRequestContext()
    .AddApiAuthentication()
    .AddApiRateLimiting()
    .AddApiLocalization()
    .AddApiControllers()
    .AddApiValidationErrors()
    .AddApiExceptionHandling()
    .AddApiStatusCodePages()
    .AddApiSwagger();

var app = builder.Build();
await app.EnsureSettingsCatalogAsync();          // paramètres confrontés au catalogue

app.UseRequestLocalization();   // 1. EN PREMIER (voir ci-dessous)
app.UseApiExceptionHandling();  // 2.
app.UseApiStatusCodePages();    // 3.
if (app.Environment.IsDevelopment()) app.UseApiSwagger();
app.UseHttpsRedirection();
app.UseApiRateLimiting();       // avant tout travail
app.UseApiAuthCookies();        // web : cookie → Authorization, juste avant l'authentification
app.UseAuthentication();
app.UseAuthorization();
app.MapApiControllers();        // MapControllers().RequireAuthorization()
app.Run();
```

- **Un sujet = une classe `<Sujet>Extensions`** dans `Extensions/` ; un middleware garde sa
  paire `Add…`/`Use…` à côté de lui dans `Middleware/`. Chaque ligne du pipeline porte un
  commentaire qui dit pourquoi elle est à cette place.
- **L'ordre du pipeline est le dispositif lui-même.** La localisation **avant** les filets
  d'erreur : la culture posée dans une méthode async ne remonte pas à l'appelant, et placés
  avant, le gestionnaire d'exceptions et les pages de statut écriraient dans la langue de la
  machine (défaut déjà rencontré : l'ordre inverse est un piège courant).
- Derrière un proxy inverse (nginx) : `UseForwardedHeaders` **tout en premier** (voir
  `authentification.md`) ; puis `UseCors` avant tout ce qui peut répondre ; puis la sonde de
  santé avant la redirection HTTPS, la limite de débit et les cookies (`production.md`).
- **Filtres globaux, dans cet ordre** : nettoyage des textes reçus, puis FluentValidation
  (`entrees-et-fichiers.md`) ; **liants** à valeur unique et d'énumération stricte en tête des
  fournisseurs MVC ; corps borné, `413` dans l'enveloppe.
- Un réglage qui remplace un défaut de MVC passe par **`PostConfigure`** (un `Configure`
  ne l'emporterait que s'il était appelé après `AddControllers`).
- Ce qui se lit dans la configuration et doit être **vérifié au démarrage** : une extension
  `IServiceCollection` de l'Infrastructure (`AddXxxSecurity` décode chaque clé et refuse de
  démarrer si elle manque ou est trop courte, **sans jamais afficher sa valeur**).

## Injection (Autofac)

- Un module par couche (`ApplicationModule`, `PersistenceModule`, `SecurityModule`…) ;
  `Extensions/ContainerExtensions` ne fait que les brancher.
- Enregistrements **un par un** (un oubli se lit dans le module), `InstancePerLifetimeScope`
  par défaut, `SingleInstance` justifié en commentaire (sans état, ou cache). Seule
  exception : les validateurs, par balayage (`AsClosedTypesOf(typeof(IValidator<>))`), car
  un validateur oublié laisserait passer une requête sans que rien ne le signale.
- `ICurrentUser` / `IRequestInfo` (couche Application) : implémentations HTTP enregistrées
  par l'API (`AddApiRequestContext`), pas par un module (qui passerait après et gagnerait).
  `SystemCurrentUser` (`UserId = null`) pour les tâches de fond.

---

## Le pattern Result et l'enveloppe

- `Domain/Common/Result.cs` : `readonly struct Result<T>` et `Result` (`IsSuccess`,
  `Error`, `Value`, conversions implicites depuis `T` et `Error`, `Map`, `Bind`, `Ensure`,
  `Tap`, `TapError`, `Match`, `With`) ; `ResultExtensions` : versions `…Async` sur
  `Task<Result<T>>`.
- `Error` (record) : `Code` (chaîne stable, jamais un enum), `Message` (déjà traduit),
  `Status` (HTTP, porté par l'erreur : seul le domaine sait *pourquoi* ça a échoué),
  `ValidationErrors`. Fabriques `NotFound`, `Validation`, `Conflict`, `Duplicate`,
  `Business`, `Unauthorized`, `Forbidden`, `TooManyRequests`, `Unexpected`.
- Codes transverses (`Domain/Common/ErrorCodes`) : `ENTITY_NOT_FOUND`, `VALIDATION_FAILED`,
  `ENTITY_CONFLICT`, `ENTITY_DUPLICATE`, `CONCURRENCY_CONFLICT`, `UNEXPECTED_ERROR`,
  `UNAUTHORIZED`, `FORBIDDEN`, `REQUEST_CANCELLED`, `TOO_MANY_REQUESTS`. Codes de protocole
  (`ROUTE_NOT_FOUND`, `METHOD_NOT_ALLOWED`, `NOT_ACCEPTABLE`, `UNSUPPORTED_MEDIA_TYPE`,
  `REQUEST_REJECTED`) dans `API/Errors/ProtocolErrorCodes` — pas dans le domaine. Codes
  d'un domaine à côté de ce domaine. Format `SCREAMING_SNAKE_CASE`.
- `ApiResponse<T> : ProblemDetails` (`Application/Common/`) : `success`, `data`,
  `errorCode`, `detail` (message traduit), `traceId`, `timestamp`, `validationErrors`,
  `metadata`. Compagnon statique `ApiResponse.From(result)` / `FromError(error)`.
- `PagedRequest` (PageNumber, PageSize borné à 200, SearchTerm, SortBy, SortDescending) et
  `PagedResult<T>` (+ `Map`).

## Un seul format d'erreur, quelle que soit l'origine

| Origine | Traitée par |
|---|---|
| Issue d'un service (`Result`) | `ApiControllerBase.Respond` → `ApiResponse.FromError` |
| Modèle invalide, JSON illisible, route/query mal formée | `InvalidModelStateResponseFactory` + `Validation/ModelStateErrors` |
| Règle FluentValidation | `Filters/FluentValidationFilter` (filtre global) → même fabrique |
| Exception imprévue | `Middleware/ExceptionHandlingMiddleware` |
| Statut sans corps (404 route inconnue, 405, 415, 429…) | `Middleware/StatusCodePagesExtensions`, `SuppressMapClientErrors = true` |

- Tous les messages de liaison (`ModelBindingMessageProvider`) traduits ;
  `AllowInputFormatterExceptionMessages = false` (pas de fuite de noms de types) ; erreur
  JSON rangée sous la propriété (`$.adresse.codePostal` → `Adresse.CodePostal`) ou sous
  `""` ; corps absent → `Validation.RequestBodyRequired`.
- 404 sans endpoint = `ROUTE_NOT_FOUND` ; 404 nu d'une action = `ENTITY_NOT_FOUND`.
- Hors MVC, l'enveloppe s'écrit par **`ApiResponseWriter`** (options JSON de MVC). **Jamais
  `Response.Clear()`** : il efface CORS et `Content-Language`, et le navigateur ne laisse
  plus lire l'erreur.
- Identifiant de corrélation **uniquement** par `HttpContext.GetTraceId()`.
- Exception imprévue : le **texte technique ne sort qu'en développement et pour un statut
  ≥ 500** ; tout statut < 500 garde son message traduit. En production : message traduit +
  `traceId`, le détail au journal. `DbUpdateConcurrencyException` → 409
  `CONCURRENCY_CONFLICT` ; annulation par le client → pas de réponse.

---

## Localisation

- Marqueur `Application/Common/SharedResource.cs` ; `Resources/Common/SharedResource.resx`
  (**français, ressource neutre** et repli) + `SharedResource.en.resx`. Ne jamais déplacer
  l'un sans l'autre (un décalage ne casse pas la compilation : les clés s'afficheraient).
- La langue vient **uniquement** d'`Accept-Language` (fr par défaut, en).
- Familles de clés : `Error.*`, `Validation.*`, `Enum.<Type>.<CODE>`,
  `Notification.<Modèle>.EmailSubject|EmailBody|Sms`. **Toute clé ajoutée en fr l'est en
  en**, dans le même geste.
- Au démarrage : chaque valeur de chaque énumération du domaine doit avoir son libellé dans
  les deux langues, sinon l'API refuse de démarrer.
- Clé de ressource ≠ texte de journal : un `logger.LogWarning("…")` s'écrit en français en
  dur.

## Énumérations en texte dans le JSON

- Le JSON écrit et lit le **nom du membre en `SNAKE_CASE` majuscule** (`PENDING_PAYMENT`),
  jamais le chiffre, par un convertisseur **strict** (`Json/StrictEnumConverterFactory`) :
  texte exact seulement. **Ne pas utiliser `JsonStringEnumConverter`** : il accepte
  `"trial"`, `" TRIAL"`, et transforme `"TRIAL, ACTIVE"` en une troisième valeur (1 | 2),
  sans erreur. Le calcul du code est à un seul endroit (`EnumCodes`), partagé par le
  convertisseur et les lookups.
- Les DTO portent **le code seul**. Les libellés sortent par **une seule route publique**,
  `GET /api/lookups` : un DTO **typé**, une propriété par liste, chacune
  `[{ code, label }]` dans la langue de la requête, dans l'ordre des valeurs stockées. Le
  front la charge une fois par langue.
- ⚠ Un paramètre de requête (`?status=…`) ne passe pas par le JSON : un liant d'énumération
  strict (même calcul `EnumCodes`) le juge, en tête des fournisseurs MVC.
- **`GET /api/lookups`** : chaque liste est `{ code, label, isActive }` (`isActive` faux pour une
  valeur retirée d'un référentiel) ; une liste qui a besoin de plus a son propre DTO, jamais un
  champ de plus sur l'élément commun.
- **Ajouter un paramètre** : sa clé typée, sa ligne `Settings` par migration, ses libellés fr/en
  et sa rubrique ; **ajouter une action d'audit** : son libellé fr/en (publié par les lookups) —
  le démarrage refuse un libellé absent.

## Swagger / OpenAPI

- Swashbuckle, **en développement seulement** (`/swagger`). `info.version` = la version de
  l'API publiée (une constante, montée à chaque changement de forme, SemVer : rupture en
  majeure, ou en mineure marquée « cassant » avant la 1.0), route `/swagger/v1/swagger.json`
  inchangée.
- ⚠ **Jamais de `[ProducesResponseType]` sur `ApiControllerBase`** : il fait disparaître le
  200 déduit de toutes les actions. Erreurs communes (400, 500) par
  `Swagger/CommonResponsesOperationFilter` ; cadenas et 401 **sur les seules routes
  protégées** par `Swagger/AuthenticationOperationFilter` (schéma `Bearer`).
- `UseAllOfToExtendReferenceSchemas()` (sinon une propriété `$ref` ne peut pas être
  `nullable`) ; `MapType<uint>` → `integer/int64` (sinon la version xmin est publiée en
  int32, trop petit).
- Les commentaires XML **sont** la documentation publiée : rien d'interne dedans (tables,
  colonnes, classes) — l'interne va dans `<remarks>` des types non exposés ou dans le code.
- Les outils de dev (`/api/dev`) : `[ApiExplorerSettings(IgnoreApi = true)]`, jamais dans le
  document.

---

## Catalogues typés : jamais de chaîne écrite à la main

| Catalogue | Élément | Contrôle au démarrage |
|---|---|---|
| `Settings/SettingKeys` | `SettingKey<T>(Key, DefaultValue)` en classes imbriquées (`SettingKeys.Otp.ValidityMinutes`) | chaque clé existe en base, `DataType` correspond, la valeur se lit |
| `Audit/AuditActions` | `AuditAction(Name, DefaultRisk)` | — (vocabulaire recopié dans le commentaire SQL de `AuditLogs.Action`) |
| `Notifications/NotificationTemplates` | `NotificationTemplate<TModel>(Name, Channels)` | textes présents en fr/en, `{Variable}` = propriété du modèle, SMS en alphabet GSM-7 |

- **Paramètres** (`ISettingsService`, singleton avec cache de 5 minutes) :
  `GetValueAsync(key, ct)` → `Result<T>` ; `GetValue(key)` synchrone, cache seul
  (rechargement en arrière-plan) ; `Invalidate()` après une modification. Ajouter un
  paramètre = une clé dans le catalogue + une ligne `Settings` par migration.
- **Audit** (`IAuditService`, par requête) : complète auteur, IP, navigateur, date, risque
  par défaut ; calcule les colonnes modifiées ; JSON PascalCase, énumérations en texte ;
  masque `***` les propriétés dont le nom contient password/secret/token/hash/recovery/otp.
- **Notifications** (`INotificationService`) : `SendEmailAsync` / `SendSmsAsync(template,
  destinataire, langue, modèle, ct)` → `Result` ; échec = `NOTIFICATION_SEND_FAILED` (503)
  dans la langue de la **requête**, texte dans la langue du **destinataire**. Email par
  MailKit (texte brut) ; en développement, **Mailpit** (Docker, SMTP `localhost:1025`,
  interface et API `localhost:8025`). Le SMS « journalisé » n'existe qu'en développement :
  hors développement, l'API refuse de démarrer sans fournisseur. ⚠ `ê â î ô û ç` font
  passer un SMS de 160 à 70 caractères.
- **Normaliseur d'identifiants** (`IIdentifierNormalizer`, singleton) : tout email
  (minuscules, ≤ 255) ou téléphone (E.164, saisie locale convertie pour les pays déclarés)
  passe par lui avant d'être comparé, cherché ou enregistré ; `null` = forme refusée.

## Configuration

- `appsettings.Development.json` (ignoré par git) : **toutes** les sections
  (`ConnectionStrings`, `Auth`, `Encryption`, `Email`…). `appsettings.json` : base seule.
  Hors développement : secrets ou variables d'environnement (`Auth__Issuer`).
- Clés générées une fois, en base64, de la bonne taille (JWT ≥ 512 bits, HMAC 256,
  chiffrement 256), écrites par un script qui n'affiche rien.
- Les messages d'erreur de démarrage disent **quelle clé** manque et **où** la mettre,
  jamais sa valeur.
