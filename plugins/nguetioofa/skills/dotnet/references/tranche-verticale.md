# Une fonctionnalité de bout en bout

Du SQL à la route, dans cet ordre. Exemples suivis, présents dans tout projet à comptes :
« Mes appareils » (`DeviceService`, `MeController`, `UserDeviceRepository`, `DeviceDto`,
`DeviceMapping`) et « Mon profil » (`AccountService`, `UpdateProfileRequest` + son validateur,
`ProfileDto`). Dans un projet existant, relire d'abord son équivalent.

Liste de contrôle d'une livraison, à cocher mentalement :

- [ ] Schéma (+ migration datée si la base existe), commentaires de table et de colonnes
- [ ] Entité, énumérations (`short`, colonne dans `<remarks>`), libellés `Enum.*` fr + en
- [ ] Configuration EF (trois sections, noms du script, `HasXminConcurrencyToken`)
- [ ] `DbSet` dans le contexte ; dépôt (contrat + implémentation) enregistré dans le module
- [ ] Requête, validateur, DTO, mapping
- [ ] Service (interface documentée + implémentation) enregistré dans le module
- [ ] Codes d'erreur du domaine ; clés `Error.*` / `Validation.*` fr + en
- [ ] Audit des actions sensibles (action ajoutée au catalogue **et** au commentaire SQL)
- [ ] Contrôleur mince, commentaires XML (`summary` + `remarks` avec chaque erreur)
- [ ] Vérifié à l'exécution : succès, chaque erreur, état de la base, nettoyage
- [ ] Tests xUnit si la règle le mérite (+ contre-épreuve)
- [ ] S'il y a un front séparé : version de l'API montée, OpenAPI exporté et relu ; plan et documentation à jour ; fichiers à committer listés

---

## Requête d'entrée (`Application/Requests/<Domaine>/`)

```csharp
/// <summary>Modification du profil : noms, langue des messages, fuseau.</summary>
/// <remarks>Ni l'email ni le téléphone : ils changent par code (<c>POST /api/me/identifiers</c>).</remarks>
public sealed record UpdateProfileRequest
{
    /// <summary>La version lue avec le profil ; rendue telle quelle.</summary>
    public uint Version { get; init; }

    /// <summary>Facultatif : certaines personnes n'ont qu'un nom.</summary>
    public string? FirstName { get; init; }

    public string LastName { get; init; } = string.Empty;
}
```

- `sealed record` à propriétés `init`. **Ni clé interne, ni colonne d'audit** : produites
  par le système. Un identifiant de ressource passe par la **route**, en `Guid publicId`.
- **Pas de DataAnnotations** de validation : FluentValidation seul.
- Nom : `<Verbe><Chose>Request` (`CreateOrderRequest`, `SearchOrderRequest : PagedRequest`).

## Validateur (`Application/Validation/<Domaine>/`)

```csharp
public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(r => r.LastName).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(localizer["Validation.Required"])
            .MaximumLength(100).WithMessage(localizer["Validation.MaxLength"]);

        // Un nom IANA seulement (Europe/Paris) : le navigateur ne connaît que ceux-là.
        RuleFor(r => r.Timezone)
            .Must(zone => zone == "UTC" || TimeZoneInfo.TryConvertIanaIdToWindowsId(zone, out _))
            .WithMessage(localizer["Validation.Timezone"]);
    }
}
```

- **Rien à enregistrer** : le module Application les trouve par balayage.
- Messages **toujours** par le localiseur. Bornes = celles du schéma.
- Une règle partagée par plusieurs requêtes (mot de passe) : une classe de règles dans le
  même dossier (`PasswordRules.NewPassword(...)`), bornes lues dans les paramètres.
- Un email ou un téléphone se valide **par le normaliseur** (`null` = forme refusée).
- Clé d'erreur = nom de la propriété ; règle sur l'objet entier = clé `""`.

## DTO (`Application/DTOs/<Domaine>/`) et mapping (`Application/Mapping/`)

```csharp
/// <summary>Un appareil de la liste « Mes appareils ».</summary>
public sealed record DeviceDto
{
    public Guid PublicId { get; init; }
    public DeviceStatus Status { get; init; }       // sort en texte : "ACTIVE"
    /// <summary>L'appareil d'où vient la requête : l'écran peut le signaler.</summary>
    public bool IsCurrent { get; init; }
}

public static class DeviceMapping
{
    public static DeviceDto ToDto(this UserDevice device, int? currentDeviceId) => new()
    {
        PublicId = device.PublicId,
        Status = device.Status,
        IsCurrent = device.Id == currentDeviceId,
    };
}
```

- **Jamais d'`Id` interne** dans un DTO : `PublicId` (ou `Code` pour un référentiel).
- **Jamais de colonne sensible** (empreinte, jeton, secret) : c'est pour ça que la
  conversion s'écrit **à la main** (pas d'AutoMapper) — une colonne ajoutée en base ne sort
  pas parce qu'une convention l'a recopiée.
- Énumérations : le **code seul**, jamais le libellé (il sort par `GET /api/lookups`).
- Bilingue : un DTO d'affichage rend le libellé dans la langue de la requête ; un DTO
  d'édition (back-office) porte les deux (`NameFr`, `NameEn`).
- Ressource modifiable : champ `Version` (`uint`), rendu à l'écriture.
- Liste : `…ListItem` et `ProjectToListItem()` sur `IQueryable`, paginée par
  `PagedResult<T>` (`Items`, `TotalCount`, `PageNumber`, `PageSize`, `TotalPages`…).

---

## Service (`Application/Services/<Domaine>/`)

Interface documentée (ce que fait chaque méthode, **chaque échec avec son code et son
statut**), implémentation `sealed` à constructeur primaire, enregistrée un par un dans le
module Application (`InstancePerLifetimeScope`, `SingleInstance` seulement si sans état ou
porteur d'un cache — et le dire en commentaire).

```csharp
public sealed class DeviceService(
    IUserDeviceRepository devices,
    IUserSessionRepository sessions,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    ICurrentUser currentUser,
    IStringLocalizer<SharedResource> localizer) : IDeviceService
{
    public Task<Result<DeviceDto>> BlockAsync(Guid devicePublicId, CancellationToken cancellationToken = default) =>
        ChangeAsync(devicePublicId, async (device, userId, now, ct) =>
        {
            if (device.Id == currentUser.DeviceId)
                return Result.Failure(Error.Conflict(localizer["Error.CannotBlockCurrentDevice"]));

            if (device.Status == DeviceStatus.Blocked)
                return Result.Success();                       // idempotent

            Close(device, DeviceStatus.Blocked, userId, now);
            var closed = await sessions.RevokeForDeviceAsync(device.Id, SessionRevocationReasons.DeviceBlocked, userId, now, ct);
            await WriteAuditAsync(AuditActions.DeviceBlocked, device, userId, closed, ct);
            return Result.Success();
        }, cancellationToken);

    /// <summary>Trouve l'appareil du compte, applique le changement dans une transaction, rend l'appareil.</summary>
    private async Task<Result<DeviceDto>> ChangeAsync(Guid devicePublicId,
        Func<UserDevice, int, DateTime, CancellationToken, Task<Result>> change, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            return Unauthorized();

        UserDevice? device = null;
        var result = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            device = await devices.FindByPublicIdAsync(userId, devicePublicId, ct);
            if (device is null)
                return Result.Failure(Error.NotFound(localizer["Error.DeviceNotFound"]));

            return await change(device, userId, UtcTime.Now(), ct);
        }, cancellationToken);

        return result.IsSuccess ? device!.ToDto(currentUser.DeviceId) : result.Error!;
    }

    private Error Unauthorized() => Error.Unauthorized(localizer["Error.Unauthorized"]);
}
```

Règles :

- **Aucune exception pour une issue attendue.** `Error.NotFound / Conflict / Duplicate /
  Validation / Business(code, …, status) / Unauthorized / Forbidden / TooManyRequests`, avec
  un message **déjà traduit**. Conversions implicites : `return entity.ToDto();` et
  `return Error.NotFound(…);` dans la même méthode.
- **Enchaîner avec les combinateurs** (`Map`, `Bind`, `Ensure`, `Tap`, `Match`, `With`, et
  `MapAsync`, `BindAsync`, `EnsureAsync`, `TapAsync` sur `Task<Result<T>>`) plutôt qu'une
  cascade de `if (x.IsFailure) return x.Error!;`. Exemple :

  ```csharp
  return await otp.IssueAsync(recipient, purpose, ct)
      .BindAsync(async issued => (await SendAsync(template, to, language, model, ct)).With(issued))
      .MapAsync(issued => new CodeSentDto(issued.ExpiresAt));
  ```

  Un `if` reste plus lisible pour une garde en tête de méthode (`currentUser.UserId is not { }`).
- **« À moi »** : l'appelant vient d'`ICurrentUser` (jamais d'un champ de la requête). La
  ressource d'un autre = 404, comme l'inexistante.
- **Idempotence** des changements d'état : bloquer un appareil bloqué réussit sans rien
  écrire.
- **Dates** : `UtcTime.Now()` (UTC tronqué à la milliseconde, la précision de la base) pour
  toute date qu'on enregistre **et** qu'on renvoie.
- **Paramètres** : `settings.GetValueAsync(SettingKeys.X.Y, ct)` → `Result<T>` ;
  `GetValue` synchrone (cache seul) avec repli sur `DefaultValue` de la clé.
- **Audit** d'une action sensible, dans la transaction (elle en suit le sort) :

  ```csharp
  await audit.WriteAsync(new AuditEntry(AuditActions.Update)
  {
      UserId = user.Id, EntityType = nameof(User), EntityId = user.PublicId,
      OldValues = before, NewValues = after,          // colonnes modifiées calculées
  }, ct);
  ```

  Tracer seulement si quelque chose a changé (`if (before != after)` sur deux types
  anonymes). Jamais de secret dedans (masquage `***` = filet, pas permission).
- **Notifications** : par le catalogue, **dans la langue du destinataire**, après
  l'enregistrement ; un échec d'envoi d'une alerte n'annule rien et va au journal.
- **Journal** : `ILogger<T>`, messages en français en dur (ils ne sont pas pour
  l'utilisateur), **jamais** d'adresse, de numéro, de code ni de contenu de message.
- Une précondition garantie par le validateur se lit en exception claire :
  `?? throw new InvalidOperationException("… aurait dû être refusé par son validateur.")`.
- Codes d'erreur propres au domaine : `static class <Domaine>ErrorCodes` à côté du service,
  chaque constante commentée (quand, quel statut, pourquoi ce statut). **Un code ne se
  renomme jamais.**

---

## Contrôleur (`API/Controllers/`)

```csharp
/// <summary>Le compte de la personne connectée : profil, appareils, sessions.</summary>
[Route("api/me")]
public sealed class MeController(IAccountService account, IDeviceService devices) : ApiControllerBase
{
    /// <summary>Modifie noms, langue des messages et fuseau.</summary>
    /// <remarks>
    /// <c>version</c> : celle reçue avec le profil. Périmée : 409 <c>CONCURRENCY_CONFLICT</c>,
    /// rien n'est écrasé.
    /// </remarks>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> Update(UpdateProfileRequest request, CancellationToken cancellationToken) =>
        Respond(await account.UpdateAsync(request, cancellationToken));

    /// <summary>Bloque un appareil.</summary>
    /// <remarks>L'appareil de la requête ne peut pas être bloqué : 409 <c>ENTITY_CONFLICT</c>.</remarks>
    [HttpPost("devices/{publicId:guid}/block")]
    public async Task<ActionResult<ApiResponse<DeviceDto>>> BlockDevice(Guid publicId, CancellationToken cancellationToken) =>
        Respond(await devices.BlockAsync(publicId, cancellationToken));
}
```

- Hérite d'`ApiControllerBase` (qui porte `[ApiController]` et `[Produces("application/json")]`).
  **Aucune règle** : appeler le service, rendre `Respond(result)`, `RespondCreated(result)`
  (201) ou `Respond(Result)` (200 avec `data` vide, jamais 204).
- Signature **typée** `ActionResult<ApiResponse<T>>` : l'OpenAPI en déduit le 200, et le
  client TypeScript en hérite.
- Commentaires XML : `summary` = ce que fait l'action ; `remarks` = règles visibles et
  **chaque erreur possible avec son code**. Ils deviennent la documentation publiée de l'API.
- Routes en anglais, pluriel, kebab-case (`/api/auth/sign-up/request`,
  `/api/me/devices/{publicId:guid}/revoke`) ; contraintes de type (`:guid`). Actions
  d'état en `POST …/revoke`, `…/block`.
- **Connexion exigée par défaut** (`MapControllers().RequireAuthorization()`) ;
  `[AllowAnonymous]` **action par action**, jamais sur la classe.
- `CancellationToken` en dernier paramètre, toujours transmis.
- Seul travail HTTP admis : ce qui touche la réponse elle-même (effacer des cookies après
  une révocation de l'appareil courant).
