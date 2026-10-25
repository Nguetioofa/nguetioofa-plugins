---
name: dotnet
description: Méthode, philosophie et conventions de Nguetioofa pour tout projet .NET / ASP.NET Core — API REST, MVC, Razor Pages, Blazor (Server, WebAssembly, Web App), tâche de fond — avec EF Core, PostgreSQL ou SQL Server, Autofac, FluentValidation, pattern Result, bilingue fr/en, authentification maison, et multi-tenant quand le système sert plusieurs organisations. À utiliser dès qu'on crée, modifie, vérifie ou relit un projet .NET de ses projets — nouveau projet, script SQL, table, entité, configuration EF, dépôt, service, validateur, contrôleur, page, composant, route, authentification, tenancy, tests, mise en service, document OpenAPI pour un frontend — même si l'utilisateur ne nomme pas le skill.
---

# Projets .NET — la méthode Nguetioofa

Ce skill décrit **comment on travaille** sur les projets .NET de Nguetioofa : la philosophie,
l'organisation du code, les conventions de nommage et de base de données, la rigueur de
vérification. Il vaut pour tous ses projets .NET et se suffit à lui-même : ses fiches
portent les modèles de code d'un projet neuf.

**Avant d'écrire un fichier d'un type donné, relire son équivalent dans le projet en cours**
(s'il en a déjà un) et l'imiter : même découpage, même densité de commentaires, même idiome.

**Le `CLAUDE.md` du projet l'emporte** sur ce skill quand ils diffèrent : un projet peut
déroger à une règle générale, et il le dit. Ce skill donne le défaut.

---

## 1. La philosophie

1. **La base fait foi (database-first).** Le schéma SQL est écrit d'abord, à la main ; le
   modèle EF le recopie, jamais l'inverse. Pas de migrations EF.
2. **La base stocke, l'API décide.** Ni fonction, ni déclencheur, ni procédure. Seulement PK,
   FK, UNIQUE, NOT NULL, valeurs par défaut, index — et un `CHECK` simple pour un invariant qui
   ne doit jamais casser (un montant positif, une date de fin après le début), comme filet :
   l'API le juge d'abord. Toute règle absente de l'API est un trou.
3. **Une issue attendue n'est pas une exception.** Introuvable, doublon, règle métier : un
   `Result` en échec, avec un code stable et un message traduit. Les exceptions sont pour
   l'imprévu.
4. **Une seule façon de rendre une issue**, succès comme échec, quelle que soit l'origine de
   l'erreur : en API, l'enveloppe `ApiResponse<T>` (le client teste `errorCode`, jamais le
   message) ; en MVC ou Blazor, une seule conversion d'un `Result` vers la vue ou le composant.
5. **Rien de visible n'est écrit en dur.** Tout texte destiné à un humain est une clé de
   ressource, en français **et** en anglais. Les messages de journal, eux, sont en français
   en dur : ils ne sont pas pour l'utilisateur.
6. **Échouer au démarrage plutôt qu'à la première requête.** Clé de configuration absente,
   traduction manquante, paramètre en base incohérent : l'API refuse de démarrer.
7. **Explicite plutôt que magique.** Pas d'AutoMapper, pas de dépôt générique, pas
   d'enregistrement par convention de nom : un oubli doit se lire dans un fichier.
8. **Un type par fichier, un sujet par classe.** Pas de fichier fourre-tout, pas de gros
   `Program.cs`, pas de gros contexte EF.
9. **Commenter seulement quand c'est vraiment nécessaire : le pourquoi non évident, jamais le
   quoi.** **Au plus 1 ligne de commentaire pour 5 lignes de code utiles**, dans chaque fichier
   (commentaires XML compris). Un record, une petite classe ou une propriété au nom clair :
   **une ligne de résumé au plus**, souvent aucune. Jamais de paraphrase, d'historique (dates,
   numéros de décision, étapes), ni de règle déjà écrite dans la conception. Un pavé de 30
   lignes au-dessus d'un record d'une ligne est une faute ; le détail va dans `docs/`.
   **Jamais de « avant on faisait…, maintenant on fait… »** (ni « désormais », « ne … plus »,
   « remplace l'ancien ») — dans le code comme dans la documentation : on décrit ce qui est ;
   l'ancien état est dans l'historique git.
10. **Vérifier avant d'affirmer.** Compiler ne prouve rien : la plupart des défauts de ces
    projets ne se voient qu'à l'exécution. On lance, on appelle, on lit la base.
11. **Un test unitaire seulement pour ce qui vaut vraiment la peine** : une règle de sécurité,
    une logique pure qu'une régression casserait sans bruit, une isolation. Jamais pour tout et
    n'importe quoi (contrôleur qui délègue, mapping, validateur de longueur, DTO, chaque cas
    d'un même chemin). Peu de tests, mais chacun avec sa contre-épreuve. **Le filet de
    non-régression, ce sont les tests d'intégration HTTP** : chaque route, son cas nominal et
    ses refus principaux, rejoués à chaque push par la CI (`verification-et-tests.md`).

---

## 2. La manière de travailler

- **Le français partout** : réponses, messages intermédiaires entre deux actions,
  commentaires, documentation, messages d'erreur. Seuls les identifiants de code sont en
  anglais.
- **Ne coder que ce qui est demandé.** Répondre à une question de cadrage n'est pas un feu
  vert. Quand un choix vient d'être validé : s'arrêter, proposer la suite, attendre.
- **Par morceaux validés.** Un gros sujet se découpe en morceaux livrables ; chacun est
  proposé, codé, vérifié à l'exécution, présenté — puis l'utilisateur valide avant le
  suivant. On annonce le découpage avant de commencer.
- **Poser la question quand la réponse change ce qu'on fait** (règle métier, arbitrage de
  sécurité, nom public). Recommander une option, dire pourquoi, ne pas trancher seul une
  règle métier. Le reste : prendre le défaut évident et le signaler.
- **Git : l'IA ne committe jamais le code**, n'indexe rien (`git add`, `git rm --cached`)
  et ne pousse rien. Elle termine en listant les fichiers à committer. Un projet peut écrire
  une exception dans son `CLAUDE.md` (ex. un dépôt que l'IA committe elle-même).
- **Secrets** : jamais dans git, jamais affichés, jamais recopiés dans la conversation —
  ni clé, ni mot de passe, ni chaîne de connexion. Pour lire ou écrire la configuration, un
  script qui ne montre aucune valeur.
- **Configuration** : `appsettings.Development.json` (ignoré par git) est **le** fichier de
  configuration, secrets ou non. `appsettings.json` garde sa seule configuration de base
  (journalisation, `AllowedHosts`) : on n'y ajoute rien.
- **Documenter d'abord** un mécanisme (connexion, tenancy, abonnement) dans `docs/`, puis
  le schéma, puis le code. Tenir à jour le plan d'implémentation (`docs/plan-…md`) : étape
  livrée = ✅, date, preuves de la vérification. **Le récit va au plan**, jamais dans le
  `CLAUDE.md` du projet, qui dit seulement ce qui est (`socle-api.md`).
- **L'auteur documente son propre travail, dans la même passe** (plan, conception, `CLAUDE.md`,
  documentation de l'API) : il a le contexte ; personne ne repassera derrière.
- **Dire la vérité sur ce qui a été vérifié** : ce qui a tourné, ce qui n'a été que compilé,
  ce qui reste à faire. Une affirmation fausse découverte plus tard se corrige
  explicitement, partout où elle a été écrite.
- **Reprendre du code d'un autre projet** quand l'utilisateur le demande : le réduire au besoin, corriger ses défauts, et dire ce qui a été changé.

---

## 3. Quatre questions avant de toucher un fichier

1. **Quel type de projet ?** API REST, MVC / Razor Pages, Blazor (Server, WebAssembly, Web App),
   tâche de fond. La présentation, la façon de rendre une erreur et la session en dépendent →
   [`references/types-de-projets.md`](references/types-de-projets.md). Les fiches marquées
   « API » ne valent que pour une API (ou l'API d'un Blazor WebAssembly).
2. **Multi-tenant ?** Seulement si le système sert plusieurs organisations isolées →
   [`references/multi-tenant.md`](references/multi-tenant.md). Sinon, ni marques de tenant, ni
   filtre, ni espace dans la route : tout ce que les fiches disent « dans un projet
   multi-tenant » se saute.
3. **Généré ou écrit à la main ?** `projectgen.json` présent et fichiers `*.g.cs` → le code
   sort de ProjectGenerator ; une correction au mauvais endroit disparaît à la régénération →
   [`references/generateur.md`](references/generateur.md). Sinon, tout s'écrit à la main.
4. **Quel niveau ?** **Complet** (données sensibles, argent, plusieurs organisations, ouvert au
   public) ou **léger** (outil interne, petit CRUD) : ce que le niveau léger s'autorise à laisser
   de côté est dans [`references/types-de-projets.md`](references/types-de-projets.md). À
   décider avec l'utilisateur à la création, et à écrire dans le `CLAUDE.md` du projet.

La philosophie, la base et les conventions sont les mêmes dans tous les cas.

---

## 4. Où mettre quoi

Solution en quatre couches, dépendances toujours dans ce sens : `Présentation (API, Web,
Blazor…) → Infrastructure → Application → Domain`. **`Domain` ne référence rien**, ni projet
ni paquet. Les lignes marquées *(API)* valent pour une API ; leur équivalent MVC et Blazor est
dans [`types-de-projets.md`](references/types-de-projets.md).

| Quoi | Où |
|---|---|
| Schéma de référence, migrations datées, contrôles | `scripts/db/<projet>-schema.sql`, `scripts/db/migrations/AAAA-MM-JJ-sujet.sql`, `scripts/db/verifications/` |
| Conception, plan | `docs/` |
| Entité (une par table) | `Domain/Entities/<Entité>.cs` |
| Énumération (`short`, valeurs du script) | `Domain/Enums/` |
| `Result`, `Error`, `ErrorCodes` transverses, marqueurs d'audit (et de tenant si multi-tenant) | `Domain/Common/` |
| Contrat de dépôt, `IUnitOfWork` | `Domain/Repositories/I<Entité>Repository.cs` |
| DTO de sortie | `Application/DTOs/<Domaine>/` |
| Requête d'entrée | `Application/Requests/<Domaine>/` |
| Validateur FluentValidation | `Application/Validation/<Domaine>/` |
| Conversion entité → DTO | `Application/Mapping/<Entité>Mapping.cs` |
| Service + son interface, codes d'erreur du domaine | `Application/Services/<Domaine>/` |
| Traductions | `Application/Resources/Common/SharedResource.resx` (fr, neutre) + `.en.resx` |
| Catalogues typés (paramètres, audit, notifications) | `Application/Settings/`, `Application/Audit/`, `Application/Notifications/` |
| Contexte EF (DbSet seulement), une configuration par entité | `Infrastructure/Persistence/`, `…/Configurations/<Entité>Configuration.cs` |
| Dépôts, intercepteurs | `Infrastructure/Persistence/Repositories/`, `…/Interceptors/` |
| Primitives de sécurité, envoi email/SMS | `Infrastructure/Security/`, `Infrastructure/Notifications/` |
| Modules Autofac, enregistrements vérifiés au démarrage | `<Couche>/Autofacs/<Sujet>Module.cs`, `Infrastructure/Autofacs/<Sujet>Registration.cs` |
| Contrôleurs (héritent d'`ApiControllerBase`) *(API)* | `API/Controllers/` |
| Vues et view models (MVC), pages (Razor Pages), composants (Blazor) | `Web/Views/`, `Web/Pages/`, `Web/Components/` |
| Une extension de démarrage par sujet | `<Présentation>/Extensions/<Sujet>Extensions.cs` |
| Middleware + sa paire `Add…`/`Use…` | `<Présentation>/Middleware/` |
| Outils de vérification, **ignorés par git** | `<Présentation>/Controllers/Dev/DevToolsController.cs` |
| Tests (xUnit), doublures une par fichier | `Tests/<Projet>.Test/`, `…/Fakes/` |

---

## 5. Déroulé d'une livraison

Pour une fonctionnalité (nouvelle table, nouvelles routes) :

1. **Cadrer** : lire le `CLAUDE.md` du projet, la conception (`docs/`), le schéma, et
   — s'il y a un frontend séparé — ses demandes et questions ouvertes. Proposer le
   découpage et les décisions à prendre ; attendre le feu vert.
2. **Base** : écrire ou modifier le schéma, puis la migration datée si la base existe
   déjà → [`references/base-de-donnees.md`](references/base-de-donnees.md).
3. **Domaine et persistance** : entité, énumérations, configuration EF, dépôt →
   [`references/ef-core.md`](references/ef-core.md).
4. **Application** : requête, validateur, DTO, mapping, service en `Result`, audit,
   traductions fr + en → [`references/tranche-verticale.md`](references/tranche-verticale.md).
5. **Présentation** : en API, contrôleur mince et documentation XML des actions →
   [`references/tranche-verticale.md`](references/tranche-verticale.md),
   [`references/socle-api.md`](references/socle-api.md) ; en MVC, Razor Pages ou Blazor, la
   page ou le composant et la conversion du `Result` →
   [`references/types-de-projets.md`](references/types-de-projets.md). Enregistrement dans le
   module Autofac. Un geste qui écrit, seul ou en lot → [`references/gestes-et-lots.md`](references/gestes-et-lots.md) ;
   ce que la demande apporte (textes, paramètres, fichiers) →
   [`references/entrees-et-fichiers.md`](references/entrees-et-fichiers.md).
6. **Vérifier à l'exécution** : chaque succès et chaque erreur documentée, la base après
   coup, puis le nettoyage. Écrire les tests xUnit seulement si la règle le mérite, et faire
   la contre-épreuve → [`references/verification-et-tests.md`](references/verification-et-tests.md).
7. **Publier pour le frontend**, s'il y a un front séparé : la version de l'API montée, le
   document OpenAPI exporté et relu (un changement non voulu est un bug), ce qui a changé et ce
   que le front doit faire — là et de la manière que le `CLAUDE.md` du projet fixe.
8. **Rendre compte** : ce qui a été fait, prouvé comment, les fichiers à committer, ce qui
   reste ; mettre à jour le plan et la documentation.

Relire [`references/pieges.md`](references/pieges.md) avant de conclure : chaque piège qui
y figure a déjà coûté cher une fois.

---

## 6. Fiches de référence

| Fiche | La lire quand |
|---|---|
| [`types-de-projets.md`](references/types-de-projets.md) | toujours d'abord : ce qui change entre API, MVC, Razor Pages, Blazor et tâche de fond |
| [`base-de-donnees.md`](references/base-de-donnees.md) | on écrit du SQL : schéma, table, migration, énumération, données initiales |
| [`ef-core.md`](references/ef-core.md) | on écrit une entité, une configuration, un dépôt, une transaction |
| [`tranche-verticale.md`](references/tranche-verticale.md) | on ajoute une fonctionnalité de bout en bout (modèles de code) |
| [`gestes-et-lots.md`](references/gestes-et-lots.md) | on écrit un geste qui change l'état, un lot, une règle d'accès, un drapeau `can…` |
| [`entrees-et-fichiers.md`](references/entrees-et-fichiers.md) | on reçoit un texte, un paramètre d'adresse, une recherche sur une personne, un fichier, une image |
| [`socle-api.md`](references/socle-api.md) | on crée une API (gabarit `assets/socle/`), ou on touche au démarrage, aux erreurs, à la localisation, à Swagger, aux catalogues ; ses sections sans enveloppe (Result, localisation, catalogues, configuration, injection) valent pour tout type |
| [`production.md`](references/production.md) | mise en service : hébergement, secrets, envois de fond, journal, sauvegardes, chiffrement au repos, données personnelles, performances |
| [`authentification.md`](references/authentification.md) | le projet a des comptes : connexion, jetons, cookies, codes, mots de passe, appareils |
| [`multi-tenant.md`](references/multi-tenant.md) | le système sert plusieurs organisations/tenants isolés |
| [`verification-et-tests.md`](references/verification-et-tests.md) | on vérifie, on écrit des tests, on outille la vérification |
| [`generateur.md`](references/generateur.md) | le projet est produit par ProjectGenerator |
| [`pieges.md`](references/pieges.md) | toujours, avant de conclure |

**Par défaut dans tout projet** : bilingue fr/en, authentification maison, xUnit.
**Multi-tenant** seulement si le système sert plusieurs organisations : à décider avec
l'utilisateur à la création du projet, car il touche le schéma de toutes les tables.
