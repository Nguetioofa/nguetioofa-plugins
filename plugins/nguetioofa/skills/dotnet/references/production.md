# Mise en service et production

Ce qui sépare une API qui marche en développement d'une API qu'une organisation réelle peut
utiliser. À trancher **avant** la première organisation réelle ; chaque sujet ouvert s'écrit
dans les décisions ouvertes du plan.

---

## Hébergement

- **Linux derrière nginx** (proxy inverse, TLS). `UseForwardedHeaders` **tout en premier**,
  confiance **au seul proxy** (`KnownProxies`, ou `KnownNetworks` en réseau Docker) ; un réseau
  de confiance trop large (`0.0.0.0/0`, un /8) se refuse au démarrage. Détail et preuves :
  `authentification.md`, « Derrière nginx ».
- **CORS** : origines permises par configuration (absente = aucune), cookies permis, **jamais
  `*`**, ni joker, ni chemin dans une origine (refus au démarrage). `UseCors` avant tout ce qui
  peut répondre, pour que chaque erreur porte ses en-têtes.
- Cookies `Secure` hors développement **quel que soit le schéma vu** (derrière le proxy,
  l'API voit du HTTP).
- **Sonde de santé** `/health`, hors du document OpenAPI : `SELECT 1` borné (3 s), `200 Healthy` /
  `503 Unhealthy`, placée avant la redirection HTTPS et la limite de débit.
- **Dépendances natives** : vérifier sous Linux tout paquet qui embarque du natif (SkiaSharp
  et son paquet `NativeAssets.Linux`, polices pour un rendu PDF : aucune police du système
  supposée présente).
- Swagger et les outils `/api/dev` **absents** hors développement (404).

## En-têtes de sécurité

Posés par l'API (un middleware, en tête du pipeline après les en-têtes transmis) ou par le proxy,
**une seule fois** — dire lequel dans la conception :

- **HSTS** hors développement (`UseHsts()`, `max-age` d'un an, `includeSubDomains` si tout le
  domaine est en HTTPS), une fois HTTPS éprouvé sur le serveur ;
- `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer`,
  `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'` sur les réponses JSON de
  l'API (une page servie par l'API — MVC, Blazor — a sa propre CSP, plus large) ;
- `Cache-Control: no-store` sur toute réponse qui porte une donnée de la personne connectée ;
- pas d'en-tête `Server` (Kestrel : `AddServerHeader = false`), ni de version du framework.

Un test d'intégration vérifie leur présence sur un 200, un 404 et un 500.

## Configuration et secrets

- Hors développement : **variables d'environnement** ou coffre de secrets (`Auth__Issuer`),
  jamais un fichier versionné. Le démarrage refuse une clé absente ou trop courte en disant
  **laquelle** et **où** la mettre, jamais sa valeur.
- `EnableSensitiveDataLogging` et le journal SQL d'EF **coupés** hors développement (ils
  écrivent les valeurs des requêtes).

## Envois et travaux hors de la requête

- **Un email ou un SMS ne part jamais pendant la requête** d'un geste qui en envoie beaucoup
  (publication à des centaines de destinataires) : le geste écrit l'intention (table d'envois
  à faire, une ligne par destinataire) **dans sa transaction**, et un travail de fond l'envoie,
  avec reprise et plafond d'essais. Un envoi isolé (code de connexion) peut partir pendant la
  requête, **après** l'enregistrement.
- Un envoi qui échoue ne change rien de ce que le geste a écrit ; il se trace, sans contenu.
- **Tâche périodique** (purge, échéances) : un `BackgroundService` à intervalle borné, **un
  passage à la fois** (verrou consultatif PostgreSQL, `pg_try_advisory_lock`, une clé par
  tâche), chaque étape isolée (une erreur va au journal, reprise au passage suivant, jamais
  l'API arrêtée), activable par configuration. Un outil de dev la déclenche, sur une base
  d'essai seulement.

## Journal et observabilité

- `ILogger<T>` partout, messages en français, **structurés** (`{OrganizationId}` en paramètre,
  jamais concaténé) ; un `traceId` par requête, le même dans la réponse et dans le journal.
- **Jamais** au journal : mot de passe, code, jeton, adresse email, numéro, contenu d'un
  message, donnée d'une personne. Une valeur à identifier s'écrit par son `PublicId`.
- Niveaux : `Microsoft.AspNetCore` à `Warning` au moins (voir `entrees-et-fichiers.md`),
  `Microsoft.EntityFrameworkCore` à `Warning` hors développement.
- Le journal applicatif part vers un collecteur choisi à la mise en service (fichier tournant,
  service hébergé) : c'est une décision du plan, pas un défaut à improviser.
- **Alerter, pas seulement tracer** : une alerte (email, messagerie de l'équipe) part sur une
  rafale d'échecs de connexion ou de verrous de comptes, une exception d'isolation entre tenants,
  un taux de `5xx` anormal, une tâche de fond qui échoue plusieurs passages de suite, un geste de
  la plateforme (suspension, changement de permissions). Une alerte dit quoi, où, et le `traceId`,
  jamais une donnée personnelle.

## Dépendances et failles connues

- Scan à chaque push (`dotnet list package --vulnerable --include-transitive`, et l'équivalent
  du front) et à chaque mise en service ; une faille haute ou critique se corrige avant la suite.
- Mises à jour des paquets regroupées, régulières (au moins à chaque phase), avec les tests
  d'intégration au vert ; une version gardée en arrière pour une raison se note, avec sa raison,
  dans le `CLAUDE.md` du projet.

## Base de données en production

- Un **rôle non superutilisateur** pour l'API (script de `GRANT`, sans toucher au schéma).
- Les migrations s'appliquent par un geste humain, après **une sauvegarde** (`pg_dump -Fc`), dans
  l'ordre des dates, avec le script de vérification ensuite (voir `base-de-donnees.md`).
- **Sauvegardes** quotidiennes, gardées hors du serveur, **restauration éprouvée** au moins une
  fois (une sauvegarde jamais restaurée ne prouve rien).

## Chiffrement au repos

- Une donnée très sensible (santé, pièce d'identité, signature) se **chiffre dans l'API** :
  AES-256-GCM, clé **dérivée** d'une clé maîtresse de configuration par usage (HKDF), la
  donnée associée liant le chiffré à sa ligne. Une seule classe chiffre et déchiffre.
- La clé maîtresse **se sauvegarde à part de la base** (une sauvegarde sans la clé ne se lit
  plus) ; son changement suit une procédure écrite (rechiffrer avec la nouvelle, garder
  l'ancienne le temps de la bascule). Ne jamais la changer sans elle. Cette procédure est
  **écrite et éprouvée sur une base d'essai avant la mise en service**, pas le jour où une clé
  fuit.
- Une lecture d'une donnée chiffrée se **trace avant le déchiffrement** ; le journal ne porte ni
  le contenu ni le nom des champs.

## Données personnelles

- Écrire dans la conception **combien de temps** chaque donnée personnelle se garde (journal,
  invitations, comptes fermés, personnes parties) et qui l'efface ; la purge est une tâche
  périodique, éprouvée sur une base d'essai.
- Un compte ne s'efface pas : il s'anonymise. Le journal d'audit ne s'efface jamais (son dépôt
  n'a ni mise à jour ni suppression, un test le garde).
- La loi applicable (pays des utilisateurs, hébergement hors du pays, données de mineurs) se
  fait relire par un juriste avant la mise en service : à noter comme décision ouverte.

## Performances

- **Une lecture de liste ne fait pas une requête par ligne** : projection (`ProjectToListItem`),
  lectures groupées (une requête par sorte de donnée, puis assemblage en mémoire), jamais une
  boucle d'appels au dépôt.
- `AsNoTracking` en lecture ; index qui commencent par la colonne de filtrage (l'unité, en
  multi-tenant) ;
  pagination côté base.
- **Mesurer sur des données de taille réelle** (un jeu de démonstration de la taille de la plus
  grosse organisation visée) avant d'affirmer qu'une liste est rapide ; une lenteur constatée
  se note dans les restes du plan avec sa mesure.
- Un rendu coûteux (PDF, image) passe par une **borne de concurrence** (N à la fois, M en
  attente, puis `503` dans l'enveloppe avec `Retry-After`) prise **avant** la transaction.
