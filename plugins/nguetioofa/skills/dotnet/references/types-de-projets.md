# Selon le type de projet : API, MVC, Razor Pages, Blazor

Le skill vaut pour tout projet .NET. **Ce qui ne change pas** d'un type à l'autre : la
philosophie, la base (`base-de-donnees.md`), EF Core (`ef-core.md`), les couches `Domain` →
`Application` → `Infrastructure`, le pattern `Result`, FluentValidation, la localisation fr/en,
les catalogues typés, les gestes (`gestes-et-lots.md`), la sécurité des comptes
(`authentification.md`, principes), les tests, la mise en service. **Ce qui change** : la couche
de présentation, la façon de rendre une erreur, la session, et ce qu'on vérifie.

Reconnaître le type avant de toucher un fichier : le `Program.cs` (`AddControllers`,
`AddControllersWithViews`, `AddRazorPages`, `AddRazorComponents`), les dossiers `Controllers/`,
`Views/`, `Pages/`, `Components/`.

Le **multi-tenant** est un choix indépendant du type : `multi-tenant.md` ne s'applique que si le
système sert plusieurs organisations isolées. Sans lui, les marques `ITenantScoped` /
`IUnitScoped`, le filtre de tenant et l'espace dans la route n'existent pas.

---

## Le niveau du projet : complet ou léger

Choisi avec l'utilisateur à la création, écrit dans le `CLAUDE.md` du projet. Un projet peut
passer de léger à complet ; l'inverse ne se fait pas.

- **Complet** : données sensibles (santé, mineurs, identité), argent, plusieurs organisations,
  ouvert au public. Tout ce skill s'applique.
- **Léger** : outil interne, petit CRUD, quelques utilisateurs connus. Il s'autorise à laisser
  de côté :
  - les catalogues typés en base (`Settings` modifiables à chaud) : `appsettings` et des
    options typées (`IOptions<T>`) suffisent ;
  - Autofac : l'injection intégrée de .NET suffit, toujours enregistrée un par un ;
  - le journal d'audit détaillé : garder la trace des seuls gestes sensibles (connexion,
    droits, suppression) ;
  - les tests d'architecture au-delà de la taille des fichiers et des `IgnoreQueryFilters()` ;
  - les outils de dev de vérification de l'isolation (pas de tenant) ;
  - la rotation fine des jetons et les appareils, si l'authentification passe par un
    fournisseur d'identité de l'organisation.

  Il **garde toujours** : la base qui fait foi, le pattern `Result` et une seule forme d'erreur,
  la validation, le bilingue si le projet l'est, les `PublicId`, une permission par action, les
  secrets hors de git, les en-têtes de sécurité, les tests d'intégration HTTP et la CI.

---

## API REST (front séparé ou application mobile)

C'est le cas que décrivent `socle-api.md` et `tranche-verticale.md` (contrôleur) : enveloppe
`ApiResponse<T>` pour tout, contrôleurs minces, OpenAPI qui est la documentation que lit le
front. Gabarit : `assets/socle/`.
- **Un front séparé** ne connaît l'API que par son document OpenAPI et ce qu'on lui en dit : ne
  rien y laisser d'interne (table, colonne, classe), monter la version à chaque changement de
  forme, dire ce qui est cassant. Deux règles rendent un ajout non cassant : **un `errorCode`
  inconnu se traite par son statut HTTP**, **un champ de réponse inconnu s'ignore**. Où et
  comment se publie ce qui change (dépôt partagé, fichier, ticket) : le `CLAUDE.md` du projet.

## MVC et Razor Pages (rendu côté serveur)

- **Couche de présentation** : `Controllers/` + `Views/` (MVC) ou `Pages/` (Razor Pages), une
  page ou une vue par écran, des **view models** dans la présentation (jamais l'entité, jamais
  le DTO d'un autre écran). Le service de l'`Application` reste le même qu'en API.
- **Rendre un `Result`** : succès → redirection (motif *Post/Redirect/Get*) avec un message
  traduit (`TempData`) ; échec de validation → `ModelState` rempli **sous les mêmes clés de
  champ**, la vue réaffichée avec la saisie ; introuvable → page 404 ; conflit de version →
  la vue réaffichée avec le message, sans écraser la saisie. Une méthode d'extension unique fait
  cette conversion (l'équivalent de `Respond`).
- **Validation** : FluentValidation appelée par le filtre global (comme en API) ou
  explicitement dans l'action, messages par le localiseur ; la validation côté client
  (`jquery-validation-unobtrusive`) n'est qu'un confort.
- **Erreurs imprévues** : `UseExceptionHandler("/Erreur")` (page traduite avec le `traceId`)
  et `UseStatusCodePagesWithReExecute("/Erreur/{0}")` ; les mêmes règles qu'en API (technique
  seulement en développement, localisation **avant** ces filets).
- **Session** : authentification par **cookie** du serveur (`AddAuthentication().AddCookie`),
  sans JWT ; les principes de `authentification.md` valent (mot de passe d'abord, une seule
  réponse, verrou, sessions en base revérifiées à chaque requête, appareils). **Jeton
  anti-falsification** (`[ValidateAntiForgeryToken]`, ou `AutoValidateAntiforgeryToken` global)
  sur tout formulaire qui écrit.
- **Langue** : la culture vient d'un cookie de langue ou de la route (`/fr/…`), choisie par la
  personne ; `Accept-Language` seulement en repli. Les vues lisent `IViewLocalizer` /
  `IStringLocalizer<SharedResource>` — aucun texte en dur.
- **Énumérations** affichées par leur libellé `Enum.<Type>.<CODE>` ; listes de choix construites
  dans le view model, jamais dans la vue.

## Blazor (Server, WebAssembly, Web App)

- **Couche de présentation** : `Components/` (pages `@page`, composants, mises en page), un
  composant par fichier, la logique dans un service de l'`Application` ou un état injecté,
  jamais dans le balisage. Paramètres `[Parameter]` en lecture seule.
- **Blazor Server / rendu interactif côté serveur** :
  - ⚠ **Le contexte EF ne s'injecte pas directement dans un composant** : un circuit vit
    longtemps et un `DbContext` n'est pas fait pour être partagé ni concurrent. Utiliser
    `IDbContextFactory<T>` (un contexte par opération, `await using`), ou des services
    `Scoped` qui en créent un par appel. Les dépôts du skill prennent alors la fabrique.
  - L'utilisateur courant vient d'`AuthenticationStateProvider`, pas d'`HttpContext` (absent
    après le premier rendu) : `ICurrentUser` s'implémente sur lui.
  - Une exception non gérée tue le circuit : chaque page qui appelle un service enveloppe
    l'appel et rend le `Result` (message traduit, saisie gardée) ; une `ErrorBoundary` dans la
    mise en page pour le reste.
- **Blazor WebAssembly** : le code tourne dans le navigateur — **aucun secret, aucune règle de
  sécurité côté client**. Il appelle une **API** du projet, qui suit alors tout ce que dit ce
  skill pour une API (enveloppe, erreurs, OpenAPI). Le client lit l'enveloppe en un seul
  endroit (l'équivalent du `unwrap` d'un front React).
- **Blazor Web App** (rendu statique + interactif) : choisir le mode de rendu **par page** et le
  dire ; un formulaire en rendu statique suit les règles MVC (anti-falsification, PRG), un
  composant interactif celles de Blazor Server ou WebAssembly.
- **Formulaires** : `EditForm` avec un validateur FluentValidation branché (un composant
  `FluentValidationValidator` maison ou une bibliothèque reconnue), messages traduits ; les
  erreurs d'un `Result` en échec se posent sur le `ValidationMessageStore` sous les mêmes clés.
  La `version` de la ressource repart avec l'enregistrement ; un conflit garde la saisie.
- **Langue** : culture choisie par la personne (cookie de culture en Server ; stockage local et
  rechargement en WebAssembly), `IStringLocalizer<SharedResource>` dans les composants.
- **Tests** : la logique dans des classes testées par xUnit comme ailleurs ; un composant qui
  porte une vraie règle (droits, état d'un assistant) se teste par **bUnit**.
- **Vérifier** dans le navigateur : le parcours, chaque erreur, fr et en, le rechargement de
  page (le circuit repart), deux onglets ouverts.

## Application de bureau ou console, tâche de fond

`Domain`, `Application`, `Infrastructure` se réutilisent tels quels ; la présentation est un
`Worker` ou une interface. `ICurrentUser` vaut `SystemCurrentUser` ; les règles de journal, de
configuration et de tâches périodiques sont celles de `production.md`.
