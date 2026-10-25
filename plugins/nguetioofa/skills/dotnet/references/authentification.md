# Authentification maison

Par défaut dans tout projet : **pas d'ASP.NET Core Identity**. Tables, jetons et sessions
sont conçus par nous, selon cette fiche. Où ranger le code : `Application/Security/`,
`Application/Services/Auth/`, `Infrastructure/Security/`, `API/Authentication/`,
`API/Middleware/AuthCookieMiddleware.cs`.

**Documenter d'abord** le mécanisme du projet dans `docs/authentification-et-comptes.md`
(principes, tables, connexion, jetons, codes, protections : les sections de cette fiche), puis
le schéma, puis le code. Ce document du projet fait foi ensuite.

---

## Principes

1. **Un compte par personne**, pour toute la plateforme ; le compte (se connecter) n'est
   pas la fiche métier (qui appartient à chaque tenant et existe sans compte).
2. **Un identifiant ne sert qu'une fois vérifié** (`EmailVerifiedAt`, `PhoneVerifiedAt`) :
   jamais de compte « non confirmé » en base. Un identifiant non vérifié = inconnu.
3. **Ne jamais dire si un compte existe** : mêmes réponses, mêmes durées, mêmes plafonds,
   que le compte existe ou non. Seul le titulaire (qui a le mot de passe, ou reçoit le
   message) apprend l'état de son compte.
4. **Les droits se lisent à chaque requête, jamais dans le jeton.** Le jeton dit *qui*,
   la session est revérifiée à chaque appel : une déconnexion ou une révocation est
   immédiate.
5. **Aucun secret en clair en base** : mots de passe (PBKDF2), codes (HMAC avec clé hors
   base), jetons de rafraîchissement (SHA-256). Le sensible du compte dans `UserSecurity`.
6. **Concurrence pensée d'emblée** : compteurs et rotations par mises à jour
   conditionnelles atomiques, vérifiées par des rafales simultanées.

## Tables

`Users` (identité, identifiants vérifiés, langue, fuseau, statut), `UserSecurity`
(empreinte du mot de passe, compteur d'échecs, `LockedUntil`, 2FA à venir), `UserDevices`
(une ligne par appareil : `Fingerprint` = identifiant d'installation, statut
ACTIVE/REVOKED/BLOCKED), `UserSessions` (`BIGINT`, `RefreshTokenHash`,
`PreviousRefreshTokenHash`, `RefreshedAt`, échéances glissante et absolue, révocation),
`OtpCodes` (`BIGINT`, empreinte, usage, destinataire, essais, consommation),
`PlatformAdministrators` (table dédiée, jamais modifiable par l'API).

---

## Mots de passe

- **PBKDF2-HMAC-SHA512, 210 000 itérations**, sel aléatoire par compte, format
  auto-descriptif `pbkdf2-sha512$210000$sel$empreinte` ; normalisation **NFKC** avant
  hachage ; résultat `SuccessRehashNeeded` → empreinte recalculée à la connexion si les
  paramètres ont été durcis.
- **Empreinte factice** calculée pour un identifiant inconnu (`SimulateVerification`) :
  même coût qu'un vrai compte.
- Règles : **8 à 128 caractères** (bornes dans `Settings`), **aucune règle de
  composition** (une phrase vaut mieux que `Mdp!2026`) ; le plafond empêche un mot de passe
  démesuré de transformer le hachage en attaque. Règle partagée (`PasswordRules`) entre
  inscription, réinitialisation et changement.

## Connexion

1. **Le mot de passe d'abord.** Puis, seulement s'il est bon : compte non verrouillé →
   compte actif → appareil non bloqué (`ACCOUNT_LOCKED`, `ACCOUNT_SUSPENDED`,
   `DEVICE_BLOCKED`, en 403, dits au seul titulaire).
2. Identifiant inconnu, non vérifié, ou mauvais mot de passe : **une seule réponse**,
   `INVALID_CREDENTIALS`, en **400** (un 401 signifie « session terminée » et ferait
   rafraîchir le client).
3. **Tout échec attend une seconde** avant de répondre (`AuthTiming.FailureDelay`) : la
   durée ne distingue aucun cas, et chaque essai coûte une seconde à qui en enchaîne.
4. **Verrouillage** : 5 échecs consécutifs → 15 minutes ; le compteur repart de zéro quand
   le verrou est posé ; rien n'est compté pendant le verrou (il ne se prolonge pas) ; un
   succès remet le compteur à zéro. Celui qui provoque le verrou reçoit la réponse
   ordinaire ; seul le titulaire apprend dans combien de minutes réessayer. Écritures par
   **mises à jour conditionnelles, hors transaction** (des tentatives simultanées ne
   s'écrasent pas).
5. **Limite par IP** en plus (le verrou protège un compte, la limite freine qui essaie
   beaucoup de comptes) : fenêtre glissante, 60 requêtes/minute/IP sur `/api/auth/*`, large
   à dessein (une entreprise ou un opérateur mobile derrière une même IP), 429 dans l'enveloppe
   avec `Retry-After`.
6. Succès : appareil reconnu ou créé, session ouverte, deux jetons, audit `Login`.
   Seuils dans `Settings`, modifiables à chaud.

## Jetons et sessions

| | Jeton d'accès | Jeton de rafraîchissement |
|---|---|---|
| Forme | JWT **HS256** | 32 octets aléatoires, opaques |
| Durée | 15 minutes | 30 jours glissants, plafond 90 jours |
| Contenu | `sub` (PublicId du compte), `sid` (PublicId de la session) + iat/exp/iss/aud — **rien d'autre** (ni tenant, ni rôle) | rien : c'est une clé |
| En base | rien | son empreinte SHA-256 |

- Validation : **une seule source** (`JwtValidation`) pour signer et vérifier ;
  `ClockSkew = 0`, HS256 seul accepté, émetteur et audience vérifiés.
- À chaque requête (`JwtBearerEvents.OnTokenValidated`) : la session `sid` est-elle active,
  du bon compte, non expirée, appareil non révoqué ? → la session active est rangée dans
  `HttpContext.Features` ; `ICurrentUser` expose `UserId`, `SessionId`, `DeviceId`.
- **Connexion exigée par défaut** : `MapControllers().RequireAuthorization()` —
  **pas** de `FallbackPolicy` (elle rendrait 401 sur les routes inconnues au lieu de 404) ;
  `[AllowAnonymous]` action par action (sur une classe, il annule le `[Authorize]` des
  actions : avertissement ASP0026).

### Rotation du jeton de rafraîchissement

- Chaque rafraîchissement **remplace** le jeton ; l'ancien va dans
  `PreviousRefreshTokenHash`. Remplacement **atomique** (compare-and-swap :
  `ExecuteUpdate … WHERE RefreshTokenHash = @lu`) : deux rafraîchissements simultanés ne
  tournent pas tous les deux.
- Politique pure (`RefreshTokenRotationPolicy` → `RefreshDecision`), testée à part :
  - jeton **courant** → `Rotate` ;
  - jeton **précédent**, dans le délai de tolérance (60 s, `Settings`) → requête sœur d'une
    rafale (rechargement de page) → `ReissueAccessOnly` : jeton d'accès seul, pas de nouveau
    jeton de rafraîchissement ;
  - jeton **précédent**, au-delà → la réponse qui portait le nouveau jeton s'est perdue →
    `RotateFromPrevious` : nouvelle rotation, **le jeton présenté redevient le précédent**,
    audit `RefreshFromPreviousToken` (risque moyen) ;
  - plus ancien → refusé.
- Le jeton précédent qui revient **n'est pas présumé volé** (la révocation déconnectait
  surtout des utilisateurs légitimes). Le prix est écrit dans les risques connus.
- Preuve attendue : une rafale de 20 requêtes simultanées → **exactement une** rotation,
  stable sur plusieurs essais.

### Web : tout en cookies

- Le front web ne manipule **aucun jeton** : trois cookies `<préfixe>.at` (accès),
  `.rt` (rafraîchissement), `.device` (identifiant d'appareil, 1 an, **survit à la
  déconnexion**). `HttpOnly`, `SameSite=Strict`, `Path=/`, `Secure` hors développement.
- **Chiffrés en AES-256-GCM** (`IEncryptionService`), clé fixe `Encryption:Key` hors base et
  hors dépôt, **nom du cookie en donnée associée** (un contenu ne se recopie pas d'un cookie
  à l'autre). Un cookie illisible est ignoré et effacé.
- Middleware **avant `UseAuthentication`** : sans en-tête `Authorization`, il lit le cookie
  d'accès ; absent ou expiré et cookie de rafraîchissement présent → il rafraîchit
  lui-même, pose les nouveaux cookies, et la requête continue. Échec → cookies effacés, 401.
  **Un 401 web veut dire « vraiment déconnecté »** : le front ne rafraîchit jamais.
- `SameSite=Strict` + JSON seul accepté (415 sinon) = pas de jeton anti-CSRF : front et API
  sous le même site (`app.…` / `api.…`), et en développement derrière le proxy du serveur
  front.

### Mobile : les jetons dans le corps

- `data.tokens` à la connexion ; stockage sécurisé (Keychain/Keystore) ; `Authorization:
  Bearer` ; `POST /api/auth/refresh` explicite, même rotation. `refreshToken: null` dans la
  réponse = **garder le sien** (cas `ReissueAccessOnly`). La plateforme déclarée à la
  connexion (`WEB`, `ANDROID`, `IOS`) décide du mode de livraison
  (`ISessionDelivery` : cookies ou corps).

## Codes à usage unique (OTP)

- 6 chiffres, **tirés sans biais** (`IKeyGeneratorService.GenerateNumericCode`), 10 minutes,
  5 essais ; au plus 3 envois en 15 minutes vers un destinataire, **tous usages confondus**
  → 429.
- Stockés en **HMAC-SHA256** de `usage\ndestinataire\ncode` (clé `Auth:OtpHmacKey`).
- **Seul le dernier code compte** ; un code accepté est consommé ; **chaque essai est
  compté en base avant la comparaison** (mise à jour conditionnelle) : 20 essais simultanés
  → 5 comptés, 10 bonnes saisies simultanées → 1 acceptée.
- **Une seule réponse** pour tout refus : `OTP_INVALID` (faux, expiré, consommé, épuisé,
  jamais demandé).
- **Le code est émis même quand il n'est pas envoyé** (compte existant à l'inscription →
  « vous avez déjà un compte » ; pas de compte au mot de passe oublié → « aucun compte » ;
  identifiant pris par un autre compte) : sinon plafond et refus trahiraient l'existence du
  compte.
- **Vérification hors transaction**, avant l'opération qu'elle autorise (annulée avec la
  transaction, l'essai ne serait plus compté) — le dépôt refuse d'être appelé dans une
  transaction.

## Mot de passe, identifiants, appareils

- **Oublié** : identifiant → code (même réponse que le compte existe ou non) → code +
  nouveau mot de passe → **toutes** les sessions fermées, verrou levé, alerte.
- **Changé** (connecté) : ancien + nouveau → les **autres** sessions fermées, celle-ci
  reste ; un mauvais mot de passe actuel = **échec de connexion** (1 s, compteur, verrou) :
  une session volée ne doit pas permettre de le deviner.
- **Alerte** après changement ou réinitialisation vers **chaque identifiant vérifié**,
  dans sa langue ; son échec n'annule rien.
- **Changer d'email/téléphone** : code vers le **nouveau** ; enregistré une fois le code
  saisi ; l'ancien reçoit une alerte ; identifiant d'un autre compte → même réponse, rien
  envoyé, `ENTITY_DUPLICATE` (409) à la confirmation.
- **Profil** : modification protégée par la `version` (409 si périmée).
- **Mes appareils** : révoquer (sessions fermées, reconnexion possible) ; bloquer (plus de
  connexion jusqu'au déblocage) — **jamais l'appareil de la requête** (409) ; révoquer
  l'appareil courant = s'y déconnecter (cookies effacés) ; appareil d'un autre compte =
  404. **Mes sessions** : la courante marquée `isCurrent`.
- `logout` ferme la session de la requête, `logout-all` toutes ; les cookies d'accès et de
  rafraîchissement sont effacés, celui de l'appareil reste.

## Derrière nginx (mise en service)

L'API ne voit plus que l'IP de nginx : **la limite par IP devient commune à toute la
plateforme** et l'audit ne montre que nginx. Correction :

- nginx : `proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;` et
  `proxy_set_header X-Forwarded-Proto $scheme;` ;
- API : `UseForwardedHeaders` **en tout premier**, `ForwardedHeaders.XForwardedFor |
  XForwardedProto`, confiance **à nginx seul** (`KnownProxies`, ou `KnownNetworks` en
  réseau Docker) — sinon n'importe qui enverrait un faux `X-Forwarded-For` à chaque requête
  et échapperait à la limite ;
- `X-Forwarded-Proto` fait savoir à l'API que le client est en HTTPS (redirection,
  cookies `Secure`) ;
- vérifier : deux IP clientes = deux limites, l'audit montre les vraies adresses.

## Récapitulatif des protections (à reprendre dans la doc du projet)

| Menace | Protection |
|---|---|
| Vol de la base | PBKDF2 salé, codes HMAC (clé hors base), jetons SHA-256 |
| Devinette de mot de passe | verrou après 5 échecs, 1 s par échec, limite par IP |
| Savoir qui est inscrit | réponses, durées et plafonds identiques |
| Vol d'un jeton de rafraîchissement | rotation, 2 générations refusées, rejeu tracé, révocation d'appareil |
| Jeton d'accès volé | 15 min de vie, session vérifiée à chaque requête |
| Script injecté (XSS) | aucun jeton lisible par la page : cookies `HttpOnly` |
| CSRF | `SameSite=Strict`, JSON seul |
| Cookie exposé (journal de proxy) | cookies chiffrés |
| Code deviné | 10 min, 5 essais comptés avant comparaison, dernier code seul, envois plafonnés |
| Énumération des identifiants | seuls des UUID aléatoires sortent |
