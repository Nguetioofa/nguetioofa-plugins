# Vérification à l'exécution et tests

**Rien ne se déclare fait sur la foi d'une compilation.** La plupart des défauts de ces
projets (mapping EF, ordre du pipeline, langue d'un message, en-tête effacé, concurrence)
compilent parfaitement et ne se voient qu'en faisant tourner l'API.

---

## Vérifier une livraison

1. **Compiler** (`dotnet build src/<Projet>.slnx`), **lancer** l'API en développement
   (`dotnet run --project src/Presentation/<Projet>.API --launch-profile http`), et lire le
   journal de démarrage : les contrôles de démarrage doivent passer.
2. **Appeler chaque point d'entrée** : le succès **et chaque erreur documentée** (validation,
   introuvable, conflit, droits, plafond). Vérifier le statut, `errorCode`, `detail`
   traduit en **fr et en** (`Accept-Language`), `validationErrors` sous le bon champ.
3. **Lire la base après coup** : la ligne écrite, l'audit tracé (et rien de secret dedans),
   les colonnes d'audit posées, rien d'écrit quand l'opération a échoué.
4. **Concurrence** quand la règle y prête (compteur, rotation, code) : une rafale de
   requêtes simultanées (20 est un bon chiffre), résultat exact attendu, sur plusieurs
   essais.
5. **Contre-épreuve** des contrôles de démarrage et des filets : injecter la faute (clé de
   traduction retirée, variable inconnue dans un texte, paramètre absent en base) et voir
   l'API refuser de démarrer, puis restaurer.
6. **Nettoyer** : base et boîte mail de test vides à la fin (`POST /api/dev/cleanup`,
   Mailpit).
7. **Rendre compte** avec les preuves : nombre de cas, ce qui a été constaté, ce qui n'a
   pas pu l'être (dit tel quel).

Outils :

- Scripts de vérification (Node `fetch`, curl, PowerShell) dans le **dossier de travail** que
  le projet désigne, sinon le répertoire temporaire de la session — jamais dans le dépôt.
- **Ne jamais compiler dans le dossier d'où tourne une API** : `dotnet build` / `dotnet test`
  avec `--artifacts-path <dossier hors du dépôt>` ; une API d'essai tourne sur un port à
  elle, avec des données de test à préfixe propre, nettoyées à la fin. Écrire le script avec l'outil d'écriture de
  fichiers (un heredoc Bash perd les antislashs des regex ; `cat > f` sans entrée bloque).
- Captures d'exemples pour la documentation : **masquer** mots de passe, jetons (tronqués
  `eyJhbGciOi…`), piles d'appels.
- Mailpit : `GET http://localhost:8025/api/v1/messages` pour vérifier un email envoyé.
- ⚠ Sous Windows, recompiler pendant que l'API tourne → erreurs de verrou de fichier
  (MSB3027) : arrêter l'API ou compiler ailleurs, ce n'est pas le code.
- ⚠ Un outil qui dit « démarré » ne prouve pas que le processus vit : lire son journal.

## Le contrôleur d'outils de développement

Un seul, **permanent et ignoré par git** : `API/Controllers/Dev/DevToolsController.cs`
(dossier `Controllers/Dev/` dans `.gitignore`). On le **complète**, on ne le recrée ni ne
l'efface à chaque vérification.

```csharp
[ApiExplorerSettings(IgnoreApi = true)]          // jamais dans Swagger, donc jamais publié
[Route("api/dev")]
public sealed class DevToolsController(AppDbContext db, IHostEnvironment environment /* … */)
    : ControllerBase, IActionFilter
{
    [NonAction]
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!environment.IsDevelopment())
            context.Result = NotFound();                 // hors développement : 404
    }

    [NonAction] public void OnActionExecuted(ActionExecutedContext context) { }

    [AllowAnonymous]                                     // action par action, jamais sur la classe
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] SeedUser request) { … }
}
```

- Il hérite de `ControllerBase` **sans** `[ApiController]` : `[FromBody]` obligatoire sur
  les corps JSON.
- Routes typiques : créer un compte à mot de passe connu, poser un code connu (plus simple
  que de lire le journal SMS de dev), verrouiller/suspendre/fermer des sessions, reculer
  une date (tester une échéance), inspecter un compte, lire l'audit anonyme, `cleanup`.
- **Données de test reconnaissables** : emails `verif-…` ou `@exemple.test` (domaine
  réservé), téléphones d'une plage dédiée ; `cleanup?since=…` efface tout ce qui les porte.

---

## Tests automatiques (xUnit)

**Tests unitaires : pour les règles qui en valent la peine, pas pour tout** (le filet de
toutes les routes, ce sont les tests d'intégration HTTP, plus bas). Méritent un test unitaire :

- les **règles de sécurité** (verrouillage, non-divulgation, rotation des jetons, codes,
  sessions fermées au bon moment) ;
- la **logique pure** qu'une régression casserait sans bruit (politique de rotation,
  normaliseur d'identifiants, calculs métier) ;
- les règles « à moi / pas à moi » (la ressource d'un autre reste introuvable **et
  intacte**).

Ne méritent pas de test : un contrôleur qui délègue, un mapping trivial, un validateur de
longueur. En conversation avec l'utilisateur, proposer les tests avant de les écrire ; un
agent qui travaille sur consigne écrit seulement ceux qui le méritent, sans en faire une liste
par sujet (un test par sujet n'est pas un but).

Organisation (`Tests/<Projet>.Test/`) :

- Arborescence miroir de la couche testée (`Services/Auth/LoginServiceTests.cs`,
  `Security/OtpServiceTests.cs`).
- **Doublures écrites à la main** dans `Fakes/`, **une par fichier** : `InMemory…Repository`
  qui **reprend les conditions SQL** du vrai dépôt (y compris les mises à jour
  conditionnelles), `PassThroughUnitOfWork`, `Recording…Service` (audit, notifications),
  `Fixed…` (utilisateur courant, provenance), `KeyLocalizer` (rend la clé, plus
  `|arguments` : un test vérifie **quel** message est choisi, pas sa traduction),
  `DefaultSettings` (défauts du catalogue). Pas de bibliothèque de mocks.
- Noms de tests **en français**, qui disent la règle :
  `On_ne_bloque_pas_l_appareil_qu_on_utilise`,
  `Revoquer_ferme_les_sessions_de_cet_appareil_seulement`. Commentaire XML de la classe =
  les règles couvertes.
- Constantes parlantes (`private const int Paul = 1; private const int Jeanne = 2;`), un
  helper de mise en place (`DeviceWithSessionsAsync(Paul, 2)`), `Assert.Single`,
  `Assert.All`.
- Vérifier le **code d'erreur** (`Assert.Equal(ErrorCodes.Conflict, result.Error?.Code)`) et
  **l'état** (rien n'a bougé), pas le texte.
- **Contre-épreuve obligatoire** : casser la règle dans le code, voir le test échouer,
  restaurer. Un test qui ne peut pas échouer ne protège rien ; un test faible se renforce.
- xUnit 2.9 : pas d'`Assert.SkipWhen` (boucle de réessai si besoin).
- Projets générés : un test qui **construit et valide le modèle EF sans base**
  (`ModeleEfTests`), lancé après chaque régénération.

Commande : `dotnet test src/<Projet>.slnx`. Rendre compte du nombre de tests qui passent.

## Tests d'intégration HTTP : le filet de non-régression

La vérification à l'exécution d'une livraison ne sert qu'une fois si ses appels sont jetés. Ce
qui doit tenir **pour toujours** s'écrit en tests d'intégration, rejoués à chaque push.

- **L'API entière en mémoire** : `WebApplicationFactory<Program>` (paquet
  `Microsoft.AspNetCore.Mvc.Testing`, `public partial class Program` dans l'API), avec le vrai
  pipeline (filtres, enveloppe, authentification, permissions) et une **vraie base** — une base
  d'essai créée depuis le schéma de référence au début de la suite et supprimée à la fin (ou un
  conteneur Testcontainers si Docker est là). Jamais le fournisseur EF en mémoire pour ces
  tests : il ne connaît ni les contraintes, ni les transactions, ni le SQL du fournisseur.
- **Pour chaque route** : le cas nominal, puis les refus principaux que sa documentation
  annonce (validation sous le bon champ, introuvable, conflit de version, permission
  manquante, ressource d'un autre — un 404) ; vérifier le statut, l'`errorCode` et l'état de la
  base, pas le texte. Une route sensible a aussi son test « sans session » et « mauvais rôle ».
- Un client par personne de test (cookies ou jeton propres), des données créées par l'API
  elle-même, un préfixe de test, et des tests **indépendants** les uns des autres.
- Ils se rangent à part (`Tests/<Projet>.IntegrationTests/`), sont plus lents que les tests
  unitaires, et c'est voulu : ils prouvent ce que l'utilisateur verra.
- Les scripts d'essai à la main (curl, Node) restent pour explorer un cas une fois ; dès qu'un
  cas mérite d'être revu à chaque changement, il devient un test d'intégration.

## Intégration continue

**À chaque push**, sur le serveur du dépôt (GitHub Actions, Azure Pipelines) : restauration,
`dotnet build` sans avertissement, tests unitaires, tests d'intégration sur une base de service
(conteneur PostgreSQL ou SQL Server du pipeline), et le **scan des failles connues** :
`dotnet list package --vulnerable --include-transitive` (une faille haute ou critique fait
échouer le pipeline). Le pipeline s'écrit à l'étape 0 d'un projet, pas à la fin ; un pipeline
rouge se répare avant toute autre étape.

## Tests d'architecture : les règles que le code ne doit jamais enfreindre

Le meilleur garde-fou d'une règle de sécurité transversale : un test qui **lit les sources**
(ou le code compilé) et refuse la forme fautive, partout, pour toujours.

- **La taille d'un fichier** : un test refuse un fichier de code de plus de 400 lignes (hors
  tests et code généré), sauf ceux d'une liste écrite avec leur raison — la règle « un sujet par
  classe » ne tient pas sans lui.
- Exemples : aucun `IgnoreQueryFilters()` sans argument ; en multi-tenant, aucune levée de
  l'isolation hors de sa **liste blanche** (fichier, table, champs qui sortent, appelants) ;
  aucun contexte EF dans la présentation ; aucune propriété `init` de création dans une demande de modification ; aucun
  caractère invisible écrit tel quel dans une source ; toute table métier marquée ; toute action
  d'écriture d'un espace porte sa règle de lecture seule.
- **Juger chaque fichier en entier, commentaires retirés**, jamais ligne par ligne : une levée
  écrite sur deux lignes est la même levée. Une forme que le détecteur ne sait pas juger (liste
  dans une variable, appel pris en délégué) se **refuse**, au lieu de passer.
- « Qui appelle » se lit dans le **code compilé** (les appels de méthode des assemblages), pas
  dans le texte.
- **Chaque détecteur a sa contre-épreuve** : la forme fautive, passée au même détecteur, doit
  échouer. Un renforcement du test garde la sienne.
- Une entrée nouvelle de liste blanche porte sa raison, et c'est une décision de l'utilisateur
  quand elle touche la sécurité.

Vérifications complémentaires au démarrage (l'API refuse de démarrer) : chaque action d'un
espace a sa permission, chaque route de plateforme son attribut, chaque écriture sa règle de
lecture seule, chaque énumération ses libellés, chaque paramètre sa ligne en base.

## Données de démonstration

- Créées **par l'application** (ses routes ou services, et `/api/dev`), jamais en SQL : elles passent par les
  mêmes règles que les vraies.
- Script **rejouable** sans fichier d'état (graine fixe, chercher avant de créer), de la **taille
  réelle** visée (la plus grosse organisation cible), sur un domaine d'exemple distinct de celui
  des données de test (que `cleanup` efface).
