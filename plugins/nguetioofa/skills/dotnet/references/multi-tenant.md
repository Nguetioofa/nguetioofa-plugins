# Multi-tenant

À appliquer **dès que le système sert plusieurs organisations** isolées les unes des
autres. Le projet écrit son contrat d'isolation en tête de son schéma (une section dédiée) et
dans sa conception (`docs/`) ; un contrat posé en base mais pas encore honoré par le code se
signale avant d'y travailler.

Vocabulaire de cette fiche : l'**organisation** (le tenant) et l'**unité** (`Units`,
`UnitId`), son second niveau — école, agence, magasin selon le projet, qui garde ses propres
noms.

---

## Le modèle

- **Le tenant** est l'organisation (groupe, entreprise). Il peut avoir un second niveau,
  l'**unité de travail** (école, agence), à la manière d'Azure DevOps (organisation →
  projet).
- **Un compte par personne, global.** Il porte N appartenances : une même personne peut
  être dans plusieurs organisations sans lien entre elles, et avoir des **rôles différents
  dans chaque unité** (gestionnaire ici, simple lecteur là). Les rôles vivent au niveau de l'unité,
  pas de l'organisation ; les administrateurs/propriétaires d'organisation ont tous droits
  sur ses unités.
- **L'identité n'est pas le profil métier** : la fiche (personnel, parent, client) existe
  par tenant, avec un `UserId` facultatif.
- Ajout de personnes par **invitation** (email/téléphone, jeton, expiration, rôles).
- L'administrateur **plateforme** (éditeur) n'est membre d'aucune organisation : table
  dédiée ; il gère offres et abonnements, ne voit pas les données métier.

## Ce que la base garantit — et rien de plus

Le filtrage est **entièrement porté par l'application** (EF Core). La base ne filtre rien
(ni RLS, ni fonction de prédicat). Elle garantit seulement :

1. **FK composites** `(OrganizationId, UnitId) → Units (OrganizationId, Id)`, ancrées
   sur `UNIQUE (OrganizationId, Id)` de la table visée : rattacher une ligne à l'unité d'un
   autre tenant est structurellement refusé, quel que soit le code appelant.
2. Un membre d'unité est forcément membre de l'organisation (FK composite vers les
   membres d'organisation, `ON DELETE CASCADE` : retiré de l'organisation, il l'est de
   toutes ses unités).

## Ce que la base attend de l'application

- Toute **table métier** porte `OrganizationId` **et** `UnitId`, **dénormalisés** même
  s'ils se déduisent par jointure (le filtre tient en une condition), référence l'unité par
  la FK composite, et place `UnitId` **en tête** de ses index de filtrage.
- Les **tables transverses** (comptes, sessions, référentiels, paramètres) n'ont pas de
  filtre de tenant : elles sont interrogées explicitement.
- Dans le code, la table métier implémente `ITenantScoped` / `IUnitScoped`.

## EF Core

- **Filtres globaux nommés** (EF Core 10) : `"Tenant"` et `"SoftDelete"`, pour lever l'un
  sans l'autre : `IgnoreQueryFilters(["SoftDelete"])`. **Ne jamais appeler
  `IgnoreQueryFilters()` sans argument** : cela lève aussi l'isolation.
- Les filtres vivent dans **un seul fichier partiel**, `<Contexte>.Filters.cs` : EF ne
  réévalue à chaque requête que les valeurs lues **sur l'instance du contexte** (l'unité de
  la requête) — une configuration d'entité ne le peut pas. C'est le seul endroit du projet
  où l'isolation se décide.
- **Estampille du tenant** par un intercepteur à l'insertion (jamais posée par un service,
  jamais lue depuis la requête du client).
- ⚠ `Database.SqlQuery` / `Database.ExecuteSql` (SQL écrit à la main) ignorent le filtre :
  y écrire la condition de l'espace soi-même. `FromSql`, `ExecuteUpdate` et `ExecuteDelete` sur
  un `DbSet` **sont** filtrés (EF y compose le filtre — à vérifier à l'exécution sur la version
  d'EF du projet), mais `ExecuteUpdate` / `ExecuteDelete` échappent à l'intercepteur (ni
  estampille, ni contrôle).
- **Intercepteur d'isolation** à l'enregistrement : à la création, les colonnes d'espace vides
  reçoivent celles de l'espace ; posées, elles doivent être de l'espace ; une ligne modifiée ou
  supprimée doit avoir été lue dans l'espace et ses colonnes d'espace ne changent jamais ; sans
  espace, tout est refusé. Un refus lève une exception dédiée (500 : c'est un défaut du code).
- **Colonnes d'espace en jetons de concurrence** : une entité rattachée par son seul `Id`, qui
  se dirait de l'espace, ne touche aucune ligne d'ailleurs. ⚠ Ce défaut du code appelant se
  présente alors comme un **conflit ordinaire** (`409 CONCURRENCY_CONFLICT`) : un 409 inattendu
  sur une ligne que personne d'autre ne modifiait fait chercher d'abord une entité rattachée
  sans avoir été lue dans l'espace.
- ⚠ Une navigation d'une table non marquée vers une table marquée est filtrée elle aussi : hors
  de l'espace, elle est vide. Lire la colonne (`…Id`) plutôt que la navigation.
- **Sans espace** (routes du compte, plateforme, tâches de fond) : le filtre ne laisse **rien**
  passer. C'est voulu : une lecture hors espace d'une table métier est une **levée**.

## Les levées de l'isolation

- **Lever** : en lecture, `IgnoreQueryFilters(["Tenant"])` **à l'endroit même de la requête**,
  sa liste écrite là (jamais dans une variable, jamais sans argument) ; en écriture, un `using`
  autour du seul enregistrement concerné. Toujours **avec un commentaire qui dit pourquoi**.
- **Le contexte EF n'est visible que de l'infrastructure** : un service de l'application ne peut
  rien lever ; l'API ne voit le contexte que dans la sonde de santé.
- **Chaque levée vit dans un dépôt dédié** qui la borne : hors espace seulement (refus avant de
  lire, sinon), sans suivi, en projection, un seul appelant, les champs qui sortent choisis un
  par un. **Une levée de plus, un champ, un appelant, un paramètre ou une condition de moins est
  une décision de l'utilisateur**, puis s'inscrit dans le test d'architecture qui les fige
  (`verification-et-tests.md`).
- Une règle qui borne une levée ne tient jamais à une seule ligne d'une requête : elle part
  d'une borne commune (`Mine()`, `InOtherUnits()`) que chaque lecture réutilise.

## L'espace de travail est dans la route

- `/api/units/{unitPublicId}/…` (l'organisation se déduit de l'unité) et
  `/api/organizations/{organizationPublicId}/…`. **Aucune route « courante »**, rien dans le
  jeton.
- Pourquoi : sur le web, les jetons sont des cookies communs à tous les onglets. Un espace
  porté par le jeton suivrait le dernier choix fait dans n'importe quel onglet (saisie dans
  la mauvaise unité) ; dans la route, chaque onglet garde le sien, et un lien s'ouvre dans
  le bon espace. **Changer d'espace = changer d'adresse**, sans appel ni nouveau jeton.
- Le dernier espace utilisé est mémorisé **par l'appareil** (côté client), pas en base.
- À chaque requête : jeton valide → session active → espace de la route → **toujours
  membre** → permissions **calculées depuis ses rôles** → l'abonnement autorise-t-il
  l'écriture ? Retirer quelqu'un prend effet à sa requête suivante.
- L'API vérifie **une permission**, jamais un nom de rôle. **Le catalogue des permissions est
  dans le code** (une énumération, un libellé fr/en par membre) ; **leur répartition par rôle
  est en base** (table des rôles). Au démarrage, l'API confronte les deux et refuse de démarrer
  sur une permission inconnue, une action sans `[HasPermission]` ou une permission d'une autre
  portée. **Aucun rôle n'est nommé dans le code** : un rôle de plus est une ligne.
- Une action ne porte **qu'une** permission ; une lecture utile à deux permissions a deux
  routes. Une migration de permissions **ajoute, n'écrase jamais** une répartition réglée.
- Les permissions de chaque espace sont renvoyées au front (liste des espaces), pour qu'il ne
  propose que les actions permises — l'API refusant de toute façon (403 `FORBIDDEN`, sans
  déconnecter). Une ressource modifiable publie aussi ses drapeaux `can…` : exactement ce que
  l'API acceptera, dans la portée de la permission qui écrit.
- Ressource d'un autre tenant = **404**, comme l'inexistante.

## Abonnement et essai

- `Subscription` côté serveur (pas de licence fichier) ; essai gratuit de durée
  configurable (`Settings`) ; une ligne par période ; statut qui décide lecture seule ou
  écriture, avec délai de grâce ; plafond d'essais par personne.
- **La période qui fait foi** se décide en un seul endroit (`WriteAccessRules`) : parmi les
  périodes en vigueur (grâce comprise), une payée prime sur un essai, puis la plus récente.
- **Lecture seule** : une organisation sans période en vigueur refuse toute écriture dans ses
  espaces (403 `ORGANIZATION_READ_ONLY`). **Toute action d'écriture d'une route d'espace déclare
  sa règle** (`[WhenReadOnly(Refused | Allowed | DecidedByService)]` : refusée par le filtre,
  permise — gestes de sécurité, lectures en `POST` —, ou jugée par le service) ; l'API refuse de
  démarrer sinon.
- **Organisation suspendue** (par la plateforme) : tout refusé à ses membres, lecture comprise ;
  le statut se relit sous le verrou de l'organisation par toute écriture hors espace qui la
  touche (accepter une invitation).

## Vérifier l'isolation

- Deux tenants de test, des données dans chacun : toute lecture, écriture et suppression
  depuis l'un sur une ressource de l'autre → 404, et la base intacte.
- Chercher `IgnoreQueryFilters()` sans argument et tout SQL brut sur une table métier à
  chaque revue — mieux : le faire refuser par un test d'architecture.
- **Sondes d'isolation** (outils de dev, base d'essai seulement) : semer des données dans deux
  unités, puis sonder chaque table marquée depuis l'autre espace et sans espace (lecture,
  `Include`, projection, jointure, SQL brut, écriture), en transaction annulée ; rapport
  « ok / ÉCHEC » par table. **Toute table marquée ajoutée entre dans la sonde.**
- Un test refuse toute table qui porte une organisation sans sa marque (`ITenantScoped` /
  `IUnitScoped`), hors une liste écrite des tables transverses.
