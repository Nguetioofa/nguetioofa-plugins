# Prompts de démarrage

Des prompts à coller **au début d'une nouvelle conversation** Claude Code, ouverte dans le dossier
du projet, avec le plugin `nguetioofa` installé. Chacun lance la même méthode : la conversation
principale est le **coordinateur** ; elle explore, cadre avec vous, écrit le plan, conçoit l'étape 0
avec vous, puis délègue aux agents `nguetioofa:dev-back`, `nguetioofa:dev-front`,
`nguetioofa:verif` et `nguetioofa:aide`.

**Remplacer** tout ce qui est entre `<…>`. Une ligne qui ne vous concerne pas se supprime.

| Situation | Prompt |
|---|---|
| Nouveau projet : API .NET seule | [A1](#a1--nouveau-projet--api-net-seule) |
| Nouveau projet : API .NET + front React | [A2](#a2--nouveau-projet--api-net--front-react) |
| Nouveau projet : application MVC ou Razor Pages | [A3](#a3--nouveau-projet--mvc-ou-razor-pages) |
| Nouveau projet : application Blazor | [A4](#a4--nouveau-projet--blazor) |
| Nouveau projet : front React sur une API qui existe déjà | [A5](#a5--nouveau-projet--front-react-sur-une-api-existante) |
| Nouveau projet : tâche de fond, worker, console | [A6](#a6--nouveau-projet--tâche-de-fond-worker-console) |
| Intégrer un projet existant à la méthode | [B1](#b1--intégrer-un-projet-existant) |
| Ouvrir une nouvelle phase d'un projet déjà intégré | [B2](#b2--ouvrir-une-nouvelle-phase) |
| Reprendre un ancien système (réécriture, migration de données) | [B3](#b3--reprendre-un-ancien-système) |
| Skill métier du projet | [C1](#c1--skill-métier-du-projet) |
| Skill du système d'UI | [C2](#c2--skill-du-système-dui) |
| Skill de marque | [C3](#c3--skill-de-marque) |
| Skill d'un partenaire ou d'une API tierce | [C4](#c4--skill-dun-partenaire-ou-dune-api-tierce) |
| Skill d'une règle transversale (réglementation, données personnelles, comptabilité) | [C5](#c5--skill-dune-règle-transversale) |

---

## Le bloc commun

Les prompts A et B se terminent par ce bloc, déjà recopié dans chacun : un prompt se colle seul.
Pour changer la méthode, changer le bloc ici **et** dans chaque prompt.

```text
Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

---

## A. Nouveau projet

### A1 — Nouveau projet : API .NET seule

```text
Nouveau projet : <Nom>, une API ASP.NET Core seule (pas de front pour l'instant), dans ce dossier.

Le besoin : <deux ou trois phrases : qui s'en sert, pour faire quoi>.
Documents existants : <cahier des charges, ancien système, maquettes… ou « aucun »>.
Base : <PostgreSQL | SQL Server>. Multi-tenant : <oui : plusieurs organisations isolées | non | à décider avec moi>.
Comptes : <administrateurs seulement | utilisateurs finaux | aucun>. Langues : fr et en.
Dépôt : <ce dossier est le dépôt | un dossier qui regroupera plusieurs dépôts>.

Charge le skill nguetioofa:dotnet (fiche types-de-projets d'abord). Puis :
1. Explore ce qui existe et lis les documents fournis ; fais au besoin une recherche du terrain
   métier (pratiques, lois, concurrents), versionnée dans docs/recherches/.
2. Cadre avec moi la phase 1 (périmètre, décisions métier), une décision à la fois.
3. Écris le plan de la phase 1.
4. Conçois avec moi l'étape 0 : le modèle bloc par bloc, puis le script SQL, éprouvé sur une base
   d'essai (exécution, rejeu refusé, vérification) avant de l'appliquer à la base de travail.
5. Pars du gabarit de socle du skill (assets/socle/) pour la solution.
Ne délègue rien avant la fin de l'étape 0.

Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

### A2 — Nouveau projet : API .NET + front React

```text
Nouveau projet : <Nom>, une API ASP.NET Core et un front React + TypeScript, dans ce dossier.

Le besoin : <deux ou trois phrases>. Écrans principaux : <liste courte>.
Documents existants : <…>. Base : <PostgreSQL | SQL Server>. Multi-tenant : <oui | non | à décider>.
Comptes : <…>. Langues : fr et en ; thèmes clair et sombre.
Organisation des dépôts, à décider avec moi en premier : un dépôt backend/, un dépôt frontend/, et
<un troisième dépôt de contrat (OpenAPI, fiches des fonctionnalités, CHANGELOG, codes d'erreur)
| le front lit le document OpenAPI exporté par le back, sans dépôt de contrat>.

Charge les skills nguetioofa:dotnet et nguetioofa:react. Puis :
1. Explore, cadre la phase 1 avec moi, écris son plan.
2. Une fonctionnalité qui touche les deux : le backend d'abord (implémenté, vérifié, publié), puis
   le front. Écris ce protocole dans le CLAUDE.md racine, et, si nous gardons un dépôt de contrat,
   son propre CLAUDE.md (qui écrit quoi, versions SemVer, statuts des fiches, questions et réponses).
3. Conçois avec moi l'étape 0 (modèle, script SQL éprouvé).
4. Avant le premier écran : propose-moi de créer le skill du système d'UI du projet (prompt C2) ;
   pour chaque écran important, écris un prompt de maquette détaillé dans
   frontend/docs/design/prompts/ avant de confier l'écran.

Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

### A3 — Nouveau projet : MVC ou Razor Pages

```text
Nouveau projet : <Nom>, une application ASP.NET Core <MVC | Razor Pages> rendue côté serveur, dans
ce dossier.

Le besoin : <…>. Écrans principaux : <…>. Base : <…>. Multi-tenant : <…>. Comptes : <…>.
Langues : fr et en (culture choisie par la personne).

Charge le skill nguetioofa:dotnet ; lis sa fiche types-de-projets.md, section « MVC et Razor
Pages » (view models, conversion d'un Result vers la vue, PRG, jeton anti-falsification,
authentification par cookie, pages d'erreur traduites). Puis :
1. Explore, cadre la phase 1 avec moi, écris son plan.
2. Conçois avec moi l'étape 0 (modèle, script SQL éprouvé).
3. Propose-moi la mise en page et les composants communs (formulaires, tableaux, messages) avant
   le premier écran ; au besoin un skill du système d'UI (prompt C2, adapté aux vues Razor).
L'agent nguetioofa:dev-back fait aussi les vues ; il n'y a pas de dev-front.

Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

### A4 — Nouveau projet : Blazor

```text
Nouveau projet : <Nom>, une application Blazor <Server | WebAssembly + API | Web App (rendu par
page)>, dans ce dossier.

Le besoin : <…>. Écrans principaux : <…>. Base : <…>. Multi-tenant : <…>. Comptes : <…>.
Langues : fr et en.

Charge le skill nguetioofa:dotnet ; lis sa fiche types-de-projets.md, section « Blazor »
(IDbContextFactory en Server, utilisateur courant par AuthenticationStateProvider, ErrorBoundary,
EditForm avec FluentValidation, bUnit pour un composant qui porte une règle ; en WebAssembly :
aucun secret ni règle de sécurité côté client, l'API suit tout le skill). Puis :
1. Explore, cadre la phase 1 avec moi (dont le mode de rendu de chaque page), écris son plan.
2. Conçois avec moi l'étape 0 (modèle, script SQL éprouvé).
3. Propose-moi les composants communs et, au besoin, un skill du système d'UI (prompt C2, adapté
   aux composants Blazor).
L'agent nguetioofa:dev-back fait aussi les composants.

Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

### A5 — Nouveau projet : front React sur une API existante

```text
Nouveau projet : <Nom>, un front React + TypeScript, dans ce dossier, sur une API qui existe déjà :
<adresse du document OpenAPI | dépôt du backend | API tierce et sa documentation>.

Le besoin : <…>. Écrans principaux : <…>. Langues : fr et en ; thèmes clair et sombre.
Session : <cookies HttpOnly posés par l'API | jeton | à découvrir dans l'API>.
Le backend : <nous le tenons aussi, dans <dossier> | il n'est pas à nous : on ne fait que le lire>.

Charge le skill nguetioofa:react. Puis :
1. Lis le document OpenAPI et fais-moi le point : routes, enveloppe des réponses, erreurs, session,
   pagination, ce qui manque pour les écrans prévus.
2. Cadre la phase 1 avec moi, écris son plan.
3. Monte le squelette du monorepo (structure.md du skill) et le client généré, sans écran.
4. Propose-moi le skill du système d'UI du projet (prompt C2) avant le premier écran.
Un besoin non couvert par l'API se note et se demande ; il ne s'invente pas.

Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

### A6 — Nouveau projet : tâche de fond, worker, console

```text
Nouveau projet : <Nom>, <un worker .NET | une application console | un service Windows>, dans ce
dossier. Il <ce qu'il fait, à quelle fréquence, sur quoi>.

Entrées : <base, fichiers, API, file de messages>. Sorties : <…>.
Échecs à prévoir : <coupure, données invalides, reprise après arrêt>.

Charge le skill nguetioofa:dotnet ; lis types-de-projets.md (« tâche de fond ») et
production.md (tâches périodiques : un passage à la fois, chaque étape isolée, reprise, journal sans
données personnelles). Puis cadre avec moi, écris le plan et conçois l'étape 0.

Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

---

## B. Projet existant

### B1 — Intégrer un projet existant

```text
Ce projet existe déjà : <Nom>, <type : API, API + front, MVC, Blazor…>. Je veux qu'il suive
désormais la méthode du plugin nguetioofa. Ne modifie aucun code dans cette conversation.

Charge les skills du plugin qui le concernent (nguetioofa:dotnet, nguetioofa:react). Puis :
1. Explore : structure, dépôts, CLAUDE.md, docs, plans, base, tests, agents et skills déjà là,
   état réel (ce qui compile, ce qui tourne). Rends-moi un état des lieux court.
2. Dis-moi où le projet s'écarte de la méthode (et où la méthode devrait apprendre de lui), sans
   rien changer.
3. Puis, avec mon accord à chaque point :
   - les CLAUDE.md réécrits selon la méthode (état en tableau court, règles par thème, récit vers
     le journal des plans, rien de ce que disent les skills du plugin), sans perdre une ligne :
     chaque bloc déplacé tel quel et vérifié par un script ;
   - .claude/regles-des-agents.md, et les anciens agents du projet retirés après sauvegarde ;
   - un skill métier du projet si ses règles de domaine sont nombreuses (prompt C1) ;
   - une mémoire courte de ce qui est décidé.
4. Propose la prochaine phase.

Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

### B2 — Ouvrir une nouvelle phase

```text
Ouvre la phase <N> de <Nom> : <sujet en une phrase>.

Lis le CLAUDE.md racine et ceux des dépôts, le plan de la phase précédente (son bilan, ses suites,
ses décisions ouvertes), la feuille de route, et la mémoire. Charge le skill reflexion-quatre-axes
s'il est là. Puis :
1. Explore (code, base, terrain métier ; recherche si utile, versionnée).
2. Cadre avec moi, une décision à la fois, en QCM.
3. Écris docs/plan-implementation-phase<N>.md (décisions, étapes, « Fait quand », estimations,
   points d'arrêt, décisions ouvertes).
4. Conçois avec moi l'étape 0 : le modèle bloc par bloc, le script SQL éprouvé sur une base
   d'essai, puis appliqué à la base de travail avec mon accord.
Seulement ensuite, délègue l'étape 1. Avant chaque étape : ses décisions prises d'abord ; pour
chaque écran important, un prompt de maquette.

Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

### B3 — Reprendre un ancien système

```text
Je veux remplacer un ancien système : <nom, technologie (ex. Blazor + SQL Server), où est son code,
où est sa base>, par <Nom> (<type de projet>).

Ne recopie pas l'ancien : il n'est pas une référence solide. Lis-le pour en tirer ce que le métier
fait réellement (règles, cas limites, données), et note ce qu'il fait mal. Puis :
1. État des lieux : fonctions, tables, volumes, règles cachées dans le code ou la base.
2. Cadre avec moi ce qu'on reprend, ce qu'on change, ce qu'on abandonne.
3. Plan de la phase 1, dont la **migration des données** comme étape à part : correspondance
   ancienne → nouvelle base, contrôles avant et après, essai sur une copie, reprise sans perte.
4. Si l'utilisateur vient de Blazor et le nouveau front est React, utilise la fiche
   blazor-vers-react du skill nguetioofa:react pour m'expliquer chaque notion nouvelle.

Méthode (à suivre, et à écrire dans les CLAUDE.md du projet) :
- Français partout ; identifiants de code en anglais.
- Tu es le coordinateur : tu mènes toi-même, avec moi, l'exploration, le cadrage, le plan et la
  conception de l'étape 0, avant de confier quoi que ce soit à un agent.
- Toute décision qui me revient se pose en question à choix (QCM), chaque option expliquée, la
  recommandée en premier avec sa raison. Une question ultra basique ou un texte affiché : tu
  tranches et tu le dis.
- Le plan (docs/plan-implementation-phaseN.md) : décisions numérotées, étapes avec « Fait quand »,
  estimation de jetons et de durée par étape et au total, décisions ouvertes à la fin. Le récit des
  étapes (dates, preuves, relectures) va dans le journal en fin de plan, jamais dans un CLAUDE.md.
- Les CLAUDE.md disent ce qui est : un tableau d'état court, les règles propres au projet par
  thème, rien de ce que disent les skills du plugin. Quand les règles d'un domaine grossissent,
  elles passent dans un skill du projet.
- Écris .claude/regles-des-agents.md : ce que les agents du plugin doivent savoir de ce projet
  (dossier de travail de chaque agent, ports, préfixe des données de test, bases d'essai et leur
  outil, qui committe quoi, où publier, ce qu'on ne lit jamais).
- Délégation : une étape à la fois, découpée en morceaux moyens ; tu annonces l'agent et l'effort
  avec sa raison ; tu contrôles chaque rapport (build, tests, exécution) avant de me le présenter ;
  relecture à froid par nguetioofa:verif de chaque étape sensible (sécurité, permissions, données,
  argent).
- Le niveau du projet (complet : données sensibles, argent, plusieurs organisations ; léger :
  outil interne) se décide avec moi dès le cadrage, et s'écrit dans le CLAUDE.md.
- Dès l'étape 0 : la CI (build, tests, scan des dépendances à chaque push) ; ensuite, chaque
  route a ses tests d'intégration HTTP (cas nominal et refus principaux) et chaque parcours
  critique son test Playwright : ce qui a été vérifié une fois se rejoue toujours.
- Ne code que ce qui est demandé ; vérifie à l'exécution avant d'affirmer ; aucun secret dans git
  ni dans la conversation ; je committe moi-même, sans ligne Co-Authored-By.
```

---

## C. Skills spécialisés du projet

Un skill du projet vit dans le dépôt qu'il sert (`backend/.claude/skills/<nom>/`,
`frontend/.claude/skills/<nom>/`) : un `SKILL.md` court (en-tête `name` et `description` qui dit
**quand** le charger), et des références lues à la demande. Les agents du plugin le chargent quand
`.claude/regles-des-agents.md` le leur dit.

### C1 — Skill métier du projet

```text
Crée le skill métier du backend de <Nom> : <nom>-metier, dans backend/.claude/skills/.

Pars des règles établies de backend/CLAUDE.md (et des documents de conception) : une référence par
domaine (<liste des domaines, ou « propose-la »>), chaque règle déplacée telle quelle — elle
désigne le seul endroit où une chose se décide — vérifié par un script qui refuse une ligne perdue.
backend/CLAUDE.md garde les règles valables partout et un index des références. SKILL.md : une
description qui dit quand le charger, le tableau des références, comment tenir les références
(une règle de domaine s'écrit là, jamais de récit). Mets à jour .claude/regles-des-agents.md pour
que nguetioofa:dev-back et nguetioofa:verif lisent les références des domaines touchés.
```

### C2 — Skill du système d'UI

```text
Crée le système d'UI de <Nom> sous forme de skill : <nom>-ui, dans frontend/.claude/skills/.

Matière : <maquettes, charte, captures, site existant, ou « à concevoir avec moi »>. Public :
<qui utilise les écrans, sur quels appareils>. Ton : <sobre, chaleureux, institutionnel…>.

Le skill contient :
- les tokens à deux niveaux (primitives, puis rôles : couleurs, espacements, rayons, ombres,
  typographie), clair et sombre, dans un tokens.css identique à celui du paquet tokens ;
- les composants maison (spécification, états, accessibilité, exemples), dans un components.css
  identique à celui du paquet ui-web ;
- les patrons d'écran (liste, fiche, formulaire, assistant, tableau de bord, connexion, état vide,
  erreur, impression), la coquille (navigation, espaces de travail) ;
- la rédaction (ton, majuscules, messages d'erreur), le dictionnaire concept → icône (Lucide),
  les règles d'accessibilité (WCAG 2.2 AA, contrastes mesurés) ;
- la réaction d'écran à chaque code d'erreur de l'API.
Décide avec moi chaque choix visuel structurant (QCM, avec un aperçu). Le skill nguetioofa:react
dit comment on écrit ; ce skill dit à quoi ça ressemble, et l'emporte pour l'apparence.
```

### C3 — Skill de marque

```text
Crée le skill de marque de <Nom> : <nom>-marque, dans frontend/.claude/skills/.

Matière : <logo, couleurs, polices, valeurs, public, pays>. Il couvre : le logo et ses variantes
(clair, sombre, monochrome, favicon), les règles d'usage et de co-marquage avec les clients, les
typographies, le ton de voix, et les supports hors produit (site public, emails, SMS, documents
imprimés). Les écrans du produit restent au skill <nom>-ui. Décide avec moi chaque choix
structurant, avec un aperçu.
```

### C4 — Skill d'un partenaire ou d'une API tierce

```text
Crée un skill pour l'intégration de <partenaire / API tierce : ex. un opérateur de Mobile Money, un
fournisseur de SMS, un service de paiement> dans <Nom> : <nom>-<partenaire>, dans
backend/.claude/skills/.

Matière : <documentation officielle (adresse), contrat, environnement de test, exemples>. Lis-la et
vérifie chaque point (rien de mémoire). Le skill dit : authentification et secrets (où ils vivent,
jamais dans git), environnements (test, production), chaque appel (demande, réponse, erreurs,
délais, limites), les webhooks et leur vérification (signature, rejeu, ordre), l'idempotence, les
états d'une opération et leurs transitions, les montants et arrondis, ce qu'on journalise ou pas,
le simulateur à écrire pour les tests, et les pièges connus. Une question que la documentation ne
tranche pas : marque-la « non documenté » et pose-la-moi.
```

### C5 — Skill d'une règle transversale

```text
Crée un skill pour <la règle transversale : protection des données personnelles, données de
mineurs, facturation et TVA, conservation légale, accessibilité réglementaire…> dans <Nom> :
<nom>-<sujet>, dans le dépôt qu'il sert.

Fais d'abord une recherche sourcée (textes officiels, pays : <pays>), versionnée dans
docs/recherches/. Le skill traduit ces textes en règles de code vérifiables : quelles données,
quelle durée, qui peut lire, ce qui se trace, ce qui s'efface, ce qu'on montre à la personne. Il dit
aussi ce qui demande l'avis d'un juriste (à noter comme décision ouverte), sans le trancher.
```
