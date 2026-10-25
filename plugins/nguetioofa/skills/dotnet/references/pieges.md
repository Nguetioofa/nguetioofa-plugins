# Pièges prouvés

Chacun a déjà coûté une fois, et presque tous **compilent sans rien dire**. Relire cette
liste avant de déclarer une livraison terminée.

---

## Pipeline et erreurs

| Piège | Symptôme | Correct |
|---|---|---|
| Filets d'erreur avant `UseRequestLocalization` | un 500 d'une requête `fr` en anglais | localisation **en premier** |
| `Response.Clear()` dans un middleware | « erreur réseau » côté navigateur (CORS et `Content-Language` effacés) | reposer statut + `ContentLength = null` |
| Deux sources de `traceId` | le `traceId` d'un 400 ne correspond pas au journal | `HttpContext.GetTraceId()` partout |
| Message technique d'exception montré en développement pour tout statut | un 409 de concurrence sorti en anglais avec la pile d'appels | technique seulement en dev **et** si statut ≥ 500 |
| `FallbackPolicy` pour exiger la connexion | les routes inconnues rendent 401 au lieu de 404 | `MapControllers().RequireAuthorization()` |
| `[AllowAnonymous]` sur la classe | il annule les `[Authorize]` des actions (ASP0026) | action par action |
| Contrôleur sans `[ApiController]` | le corps JSON n'est pas lié, pas de validation automatique | hériter d'`ApiControllerBase`, ou `[FromBody]` explicite |
| `[ProducesResponseType]` sur le contrôleur de base | le 200 disparaît de toutes les actions dans l'OpenAPI | filtre Swagger pour les erreurs communes |
| Réglage MVC par `Configure` | ignoré selon l'ordre d'appel | `PostConfigure` |

## JSON et OpenAPI

| Piège | Symptôme | Correct |
|---|---|---|
| `JsonStringEnumConverter` | `"trial"`, `" TRIAL"` acceptés ; `"TRIAL, ACTIVE"` devient une 3ᵉ valeur | convertisseur strict (`StrictEnumConverterFactory`) |
| Nom de colonne dans le `<summary>` d'une enum ou d'un DTO | la structure de la base publiée au frontend | l'interne dans `<remarks>` |
| `$ref` d'une propriété facultative | publiée non `nullable` | `UseAllOfToExtendReferenceSchemas()` |
| `uint` (version `xmin`) | publié en `int32`, trop petit | `MapType<uint>` → `int64` |
| Messages de liaison du framework | anglais, noms de types (`$.champ`, `System.Int32`) | `ModelBindingMessageProvider` traduit, `AllowInputFormatterExceptionMessages = false` |
| Paramètre de requête énuméré (`?status=`) | ne passe pas par le convertisseur JSON | liaison à traiter au premier filtre |

## EF Core et base

| Piège | Symptôme | Correct |
|---|---|---|
| `HasDefaultValue` pour un défaut non généré par la base | un `false` / `0` égal au défaut du type n'est jamais envoyé | défaut posé par l'entité ; `HasDefaultValueSql` seulement pour `now()` / `gen_random_uuid()` |
| `ForeignKeyIndexConvention` laissée active | EF invente des index absents du script | la retirer dans `ConfigureConventions` |
| JSONB non déclaré | PostgreSQL refuse l'écriture (`text`) | `HasColumnType("jsonb")` |
| `DateTime.UtcNow` enregistré puis renvoyé | 7 décimales rendues, 3 stockées : la relecture diffère | `UtcTime.Now()` (tronqué à la ms) |
| Date locale | Npgsql refuse une date non UTC en `TIMESTAMPTZ` | UTC partout |
| `IgnoreQueryFilters()` sans argument | l'isolation multi-tenant saute avec la suppression logique | filtres nommés, `IgnoreQueryFilters(["SoftDelete"])` |
| `ExecuteUpdate` / `ExecuteDelete` / SQL brut | ni audit, ni filtres ; `ExecuteDelete` supprime vraiment | audit et filtre de tenant à la main |
| `EnableRetryOnFailure` | interdit les transactions du code | ne pas l'activer |
| Lire-modifier-écrire un compteur disputé | des essais simultanés dépassent le plafond | `ExecuteUpdate` conditionnel (compare-and-swap) |
| Essai d'un code vérifié dans une transaction | annulé avec elle, il n'est plus compté | vérification **hors** transaction, le dépôt le refuse |
| Configuration EF manquante | EF mappe l'entité par convention (`Order` au lieu de `Orders`) | écrire les configurations de toutes les tables d'un lot |
| `Result` est une struct | l'inférence d'une lambda qui rend `Task<Result>` hésite | `Result.Failure(error)` explicite |
| Identité dans une FK composite sans `ValueGeneratedOnAdd()` | `INSERT` avec `Id = 0`, erreur `428C9` | `ValueGeneratedOnAdd()` et un test qui le garde |
| Ancre de FK composite non déclarée | EF invente une clé `AK_…` absente du script | `HasAlternateKey(…).HasName("UQ_…")` |
| `byte[]` modifié en place | EF ne voit pas le changement | un tableau neuf |
| `409` inattendu sur une ligne que personne ne modifie | une entité rattachée sans avoir été lue dans l'espace (jetons d'espace) | relire dans l'espace, suivie |
| `SqlQuery` / `ExecuteSql` sur une table métier | le filtre de tenant ne s'applique pas | condition de l'espace écrite à la main |
| `texte[..n]` pour couper | moitié d'émoji, la base refuse (500) | `Truncate` |
| Paramètre d'adresse répété | la première valeur gardée sans rien dire | liant à valeur unique (400) |
| `(page - 1) * taille` | dépassement d'entier sur une page énorme | `ItemsToSkip(page, taille)`, page bornée |
| `Microsoft.AspNetCore` sous `Warning` | les jetons des adresses dans le journal | `Warning` au moins, routes à jeton par leur modèle |
| Emails envoyés pendant une requête de lot | requête lente, envoi perdu sur une coupure | intention écrite dans la transaction, envoi par un travail de fond |

## Sécurité

| Piège | Symptôme | Correct |
|---|---|---|
| Messages distincts « compte inconnu » / « mauvais mot de passe » | on sonde qui est inscrit | une seule réponse, même durée (1 s), même plafond |
| « Compte verrouillé » dit à n'importe qui | confirme que le compte existe | seulement après le bon mot de passe |
| 401 pour un échec de connexion | le client croit sa session expirée et rafraîchit | 400 `INVALID_CREDENTIALS` |
| Code envoyé seulement si le compte existe | le plafond d'envois trahit l'existence du compte | émis toujours, envoyé ou non |
| Limite par IP derrière nginx sans en-têtes transmis | une seule limite pour toute la plateforme | `UseForwardedHeaders` en premier, confiance à nginx seul |
| `X-Forwarded-For` cru de n'importe qui | contournement de la limite par IP | `KnownProxies` / `KnownNetworks` |
| Fenêtre glissante sans `Retry-After` | le 429 ne dit pas quand réessayer | repli sur la durée d'une tranche (commentaire honnête) |
| SMS avec `ê â î ô û ç` | 70 caractères au lieu de 160, plusieurs SMS facturés | contrôle GSM-7 au démarrage, reformuler |
| Code ou numéro au journal | secret lisible par qui lit les journaux | jamais d'adresse, de numéro ni de contenu (sauf le faux SMS **de dev**, qui le fait exprès — le dire) |

## Outillage

| Piège | Symptôme | Correct |
|---|---|---|
| Recompiler avec l'API lancée (Windows) | erreurs MSB3027 (fichiers verrouillés) | compiler ailleurs (`--artifacts-path`) ou arrêter l'API ; ce n'est pas le code |
| Outil de lancement qui dit « démarré » | le processus est mort au démarrage | lire le journal du processus |
| `cat > fichier` sans entrée | la commande reste bloquée | écrire le fichier avec l'outil d'écriture |
| Heredoc Bash → Node avec regex | antislashs perdus | écrire le script avec l'outil d'écriture |
| `psql -c` avec accents (Git Bash sous Windows) | octets non UTF-8, requête corrompue | `-f fichier` + `PGCLIENTENCODING=UTF8` |
| `git rm --cached` / `git add` dans le dépôt de l'utilisateur | modification de son index | ne jamais toucher à l'index du code ; si c'est fait, `git reset HEAD -- fichier` et le dire |
| xUnit 2.9 | `Assert.SkipWhen` n'existe pas | boucle de réessai |
| Test sans contre-épreuve | il passe aussi quand la règle est cassée | casser, voir échouer, restaurer |
| Affirmation non vérifiée écrite dans la doc | elle se propage (CLAUDE.md, mémoire, commentaires) | vérifier avant d'écrire ; corriger partout et le dire |
