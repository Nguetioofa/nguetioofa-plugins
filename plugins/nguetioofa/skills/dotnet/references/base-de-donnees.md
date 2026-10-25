# Base de données

**Relire le schéma du projet avant d'écrire une table** : son en-tête porte les règles
ci-dessous et leurs raisons. Un projet neuf écrit cet en-tête d'abord, d'après cette fiche.

---

## Fichiers

| Fichier | Rôle |
|---|---|
| `scripts/db/00-creation-base.sql` | crée la base (UTF-8, collation de tri, fuseau UTC) — une fois par serveur |
| `scripts/db/<projet>-schema.sql` | **le** schéma complet, sur base vide, dans une transaction |
| `scripts/db/migrations/AAAA-MM-JJ-sujet.sql` | chaque évolution, dans l'ordre des dates, jamais modifiée une fois appliquée |
| `scripts/db/verifications/<phase>-verification.sql` | contrôles du schéma, dans une transaction annulée |
| `scripts/db/README.md` | installer, écrire une migration, vérifier |

Le schéma de référence est **toujours** l'état final : une migration est répercutée dedans,
pour qu'une base neuve naisse directement à jour. **Aucun `ALTER TABLE` de rattrapage**
dans le schéma : il s'exécute d'un bout à l'autre sur une base vide.

---

## Organisation du fichier de schéma

- En-tête long : but, exécution, organisation en sections, nommage, clés, isolation,
  parti pris « base qui stocke », bilingue, dates, types, tri, suppression et audit,
  encodage (**UTF-8 sans BOM** : psql lirait le BOM comme un caractère parasite).
- `SET client_encoding = 'UTF8';` puis `BEGIN;` … `COMMIT;` : une erreur laisse la base
  vide, jamais à moitié créée (PostgreSQL annule aussi le DDL). Lancer avec
  `psql -v ON_ERROR_STOP=1`.
- Garde-fous en tête (version minimale, encodage, fuseau) et **contrôle final** en fin :
  le nombre de tables attendu, un message `« <Projet>, phase N : X tables créées, contrôle
  final réussi. »`.
- **Sections numérotées dans l'ordre des dépendances** : suivi des scripts, contrat
  d'isolation, référentiels, tenants et utilisateurs, sécurité, appartenances, …, index,
  données initiales, contrôle final.
- **Au-dessus de chaque table**, un bloc :

```sql
-- ---------------------------------------------------------------------------
-- UserDevices - appareils d'un compte : navigateur, téléphone, tablette.
--
-- Dépend de      : Users (CASCADE)
-- Référencée par : UserSessions
--
-- Pourquoi la table est faite ainsi : unicité, cycle de vie, choix non évidents.
-- ---------------------------------------------------------------------------
```

- **En regard des colonnes**, un commentaire court quand le nom ne suffit pas ; les
  énumérations documentées **au-dessus** de leur colonne : `-- 1=ACTIVE  2=REVOKED  3=BLOCKED`.
- Colonnes groupées par thème avec un commentaire de groupe (`-- Confiance et révocation`),
  les contraintes en fin de table, alignées.

---

## Nommage

- Tables et colonnes **en anglais, PascalCase**. Tables au pluriel (`Users`,
  `OrganizationMembers`), entités C# au singulier.
- Contraintes `PK_<Table>`, `UQ_<Table>_<Colonnes>`, `FK_<Table>_<TableVisée>` ; index
  `IX_<Table>_<Sujet>`. Ces noms sont **utilisés par l'API** (une violation de
  `UQ_Organizations_Slug` devient « identifiant d'URL déjà pris ») : les choisir parlants.
- **PostgreSQL** : tout nom est cité (`"Users"."Email"`), sinon il passe en minuscules. EF
  cite de lui-même ; à la main, toujours citer. Pas de `DF_` (PostgreSQL ne nomme pas les
  valeurs par défaut).
- **SQL Server** : `DF_<Table>_<Colonne>` pour les valeurs par défaut.

---

## Clés

- **Chaque table a sa colonne `Id` en clé primaire. Aucune clé primaire composite** : une
  table d'association a son propre `Id`, et le couple qu'elle relie est protégé par un
  `UNIQUE`. Même une clé naturelle mono-colonne est remplacée par `Id` + `UNIQUE`.
- `Id` = `SMALLINT` / `INT` / `BIGINT` **identity** (`GENERATED ALWAYS AS IDENTITY`,
  `IDENTITY(1,1)`) : clé **interne**, toutes les jointures et FK l'utilisent, **jamais
  exposée**. `BIGINT` pour les tables à forte rotation (sessions, codes, audit).
- `PublicId` = UUID **aléatoire** (`gen_random_uuid()`, `NEWID()`), `UNIQUE` : la **seule**
  valeur qui sort par les URL, l'API et les jetons. Imprévisible : aucune énumération.
  Posée seulement sur les **racines adressables** directement ; une table fille s'atteint
  par son parent.
- Les **référentiels** s'exposent par leur `Code` (`MANAGER`, `STANDARD`), pas de
  `PublicId`.
- Les **tables techniques** (paramètres, audit, sessions, codes) n'ont pas de `PublicId` :
  jamais exposées.

---

## Aucune logique en base, un filet pour les invariants

- Interdits : fonction, déclencheur, procédure, vue matérialisée de calcul, `CHECK` qui porte
  une règle métier ou un format (ni regex, ni `lower()`, ni liste de valeurs d'une énumération :
  elle change avec le code), `COMMENT ON` (les commentaires vivent **dans le fichier**, jamais
  dans le catalogue).
- Permis : PK, FK, UNIQUE, NOT NULL, DEFAULT, index (y compris **filtrés** :
  `WHERE "IsDeleted" = false`), et un **`CHECK` simple sur une seule ligne** pour un invariant
  qui ne doit jamais casser, quelle que soit la règle métier du moment : un montant ou une
  quantité positive, une date de fin après le début, un pourcentage entre 0 et 100. Nommé
  `CK_<Table>_<Sujet>`, déclaré aussi dans la configuration EF
  (`ToTable(t => t.HasCheckConstraint("CK_…", "…"))`) pour que la comparaison au catalogue le
  voie.
- **L'API juge d'abord**, avec un message clair : un `CHECK` qui refuse une écriture est un bug
  de l'API (un `500`, au journal), jamais une réponse prévue. Les validateurs restent la seule
  barrière que voit l'utilisateur — ils se testent comme tels ; le `CHECK` rattrape ce qu'ils
  ont laissé passer, et ce qu'un script écrit à la main.

---

## Types

| PostgreSQL | SQL Server | Usage |
|---|---|---|
| `TIMESTAMPTZ(3)` | `DATETIME2(3)` | **toutes** les dates, en UTC, à la milliseconde |
| `VARCHAR(n)` | `NVARCHAR(n)` | texte borné (en UTF-8, tout est Unicode) |
| `TEXT` | `NVARCHAR(MAX)` | texte long |
| `SMALLINT` | `TINYINT` / `SMALLINT` | énumérations |
| `BOOLEAN` | `BIT` | |
| `NUMERIC(12,2)` | `DECIMAL(12,2)` | montants |
| `BYTEA` | `VARBINARY` | empreintes, secrets chiffrés |
| `JSONB` | `NVARCHAR(MAX)` | valeurs d'audit, données souples |
| `UUID` | `UNIQUEIDENTIFIER` | `PublicId` |
| `CHAR(n)` | `CHAR(n)` | codes de longueur fixe (pays, devise) |

- Longueurs de référence : email 255, téléphone E.164 20, nom 100, IP 45, user-agent 512,
  motif 255, langue `VARCHAR(5)`, fuseau IANA 50.
- **Jamais de `TIMESTAMP` sans fuseau** (PostgreSQL) : il ne sait pas d'où vient l'heure.

---

## Colonnes récurrentes

- **Énumérations** : `SMALLINT NOT NULL DEFAULT 1`, valeurs commentées au-dessus, reflétées
  par une `enum C# : short`. **Ne jamais renuméroter** une valeur stockée.
- **Bloc d'audit** des tables principales, dans cet ordre :

```sql
    "CreatedAt"      TIMESTAMPTZ(3) NOT NULL DEFAULT now(),
    "CreatedBy"      INT            NULL,      -- sans FK : trace historique
    "UpdatedAt"      TIMESTAMPTZ(3) NULL,
    "UpdatedBy"      INT            NULL,
    "IsDeleted"      BOOLEAN        NOT NULL DEFAULT false,
    "DeletedAt"      TIMESTAMPTZ(3) NULL,
    "DeletedBy"      INT            NULL,
    "DeletionReason" VARCHAR(255)   NULL,
```

  Les colonnes `…By` **n'ont pas de FK** : une trace n'a rien à garantir. Une table dont les
  lignes ne changent jamais n'a que `CreatedAt`.
- **Suppression logique** (`IsDeleted`). Un compte ne s'efface jamais : on l'anonymise.
- **Bilingue** : toute donnée **publique** stockée en base existe en `…Fr` et `…En`
  (`NameFr`, `NameEn`, `DescriptionFr`…), les noms `NOT NULL` dans les deux langues. Deux
  colonnes plutôt qu'une table de traductions : langues fixées, ligne lue d'un coup. Les
  tables **système** (`Settings`, `AuditLogs`) restent en français. Chaque compte, tenant ou
  établissement porte sa **langue de communication** (`PreferredLanguage`).
- **Secrets** : jamais en clair. Mots de passe, codes, jetons → leurs empreintes ; tout ce
  qui est sensible du compte dans une table à part (`UserSecurity`), pas dans `Users`.
- **Identifiants** : email en minuscules, téléphone en E.164 — normalisés **par l'API**.

---

## Tables imposées dans tout projet

- `AppliedScripts` (`Id`, `Name` UNIQUE, `AppliedAt`) : suivi des migrations.
- `Settings` : paramètres clé/valeur typés (`Key` UNIQUE, `Value`, `DataType`,
  `Description`, `IsEncrypted`, `AllowedValues`, `ValidationRegex`…), `Id INT`, sans
  `PublicId`, en français. Toute clé a son pendant typé dans le code (voir
  `socle-api.md`).
- `AuditLogs` : `Id BIGINT`, `Action`, `EntityType`, `EntityId`, `EntityDisplayName`,
  `OldValues`/`NewValues` (JSON), `ChangedColumns`, `RiskLevel`, `IpAddress`, `UserAgent`,
  auteur, tenant ; en français. Le vocabulaire des actions est dans le commentaire de la
  colonne **et** dans le catalogue C#.

---

## Création de la base (PostgreSQL)

- **PostgreSQL 15 minimum** (collation ICU à la création), 17 ou 18 recommandé.
- Collation ICU au niveau **de la base**, jamais par colonne : `fr-<PAYS>-u-kn`, le pays
  du marché du projet (`fr-FR-u-kn`…) — é trié avec e, « Lot 2 » avant « Lot 10 ». `ALTER DATABASE … SET timezone TO 'UTC'`.
- `CREATE DATABASE` hors transaction : dans pgAdmin, instructions **séparées**.
- **Un seul compte, `postgres`**, en développement — choix de l'utilisateur, risque signalé
  une fois. En production, un rôle non superutilisateur par un script de `GRANT`, sans
  toucher au schéma.

---

## Écrire une migration

```sql
-- =============================================================================
--  AAAA-MM-JJ - sujet en une ligne
--
--  Pourquoi ce changement (renvoi à la conception, § n).
--
--  ENCODAGE : UTF-8 sans BOM.
-- =============================================================================

SET client_encoding = 'UTF8';

BEGIN;

-- En PREMIER : rejoué, le script échoue ici, avant de toucher à quoi que ce soit.
INSERT INTO "AppliedScripts" ("Name") VALUES ('AAAA-MM-JJ-sujet');

-- … les changements …

COMMIT;
```

- Une transaction ; répercuter dans le schéma ; mettre à jour le contrôle final si une
  table s'ajoute ; documenter toute nouvelle valeur d'énumération dans le commentaire de sa
  colonne (et dans les deux fichiers).
- **Qui l'applique** : le `CLAUDE.md` du projet le dit ; à défaut, l'IA l'écrit et
  l'utilisateur l'exécute (le lui rappeler).
- **Toute migration s'éprouve avant d'être appliquée**, sur des bases d'essai nommées à part
  (`<base>_verif_…`), jamais sur la base commune :
  1. sur une **copie** de la base (`pg_dump` / `pg_restore`, ou `CREATE DATABASE … TEMPLATE`) :
     appliquée, puis le script de vérification vert, puis l'API lancée dessus ;
  2. sur une **base neuve** (schéma de référence) : mêmes tables, colonnes, clés et index que
     la copie migrée (comparer le catalogue) ;
  3. **rejouée** : refusée par sa première ligne (`AppliedScripts`), rien de touché.
- **Non cassante** (ajouts, données de départ : l'API en marche continue de fonctionner) ou
  **cassante** (retrait ou changement d'une colonne que l'API lit) : une migration cassante se
  coordonne avec l'arrêt de l'API et le déploiement du code qui la suit. Une colonne se retire
  en deux temps : le code cesse de la lire, puis la migration la retire.
- Un paramètre ajouté au catalogue de code = une ligne `INSERT INTO "Settings"` dans une
  migration : sinon l'API refuse de démarrer.

---

## Vérifier

- Script de vérification : chaque contrôle affiche `OK` ou lève `ÉCHEC`, le tout dans une
  transaction **annulée** (la base est rendue intacte). Un contrôle qui ne peut pas échouer
  ne prouve rien : l'éprouver en injectant l'erreur qu'il doit attraper.
- Comparer le modèle EF au catalogue (`information_schema`, `pg_index`, `pg_constraint`)
  après chaque lot de configurations : 0 écart attendu.
- ⚠ **Sous Windows (Git Bash)** : pas d'accents dans `psql -c "…"` (octets non UTF-8). Passer
  par un fichier (`-f`) avec `PGCLIENTENCODING=UTF8`. psql n'y est souvent pas dans le PATH
  (dossier `bin` de l'installation de PostgreSQL).
- **Mot de passe** : par le fichier `pgpass` de l'utilisateur (`~/.pgpass`,
  `%APPDATA%\postgresql\pgpass.conf` sous Windows), jamais dans une commande, un script ou
  la conversation.
