# Session, espaces, droits

Ce que l'écran sait de la personne connectée, et comment il décide de ce qu'il propose.
L'API reste seule juge : l'écran ne fait que ne pas proposer ce qu'elle refuserait.

---

## La session sur le web

- **Aucun jeton visible** : l'API pose des cookies `HttpOnly` ; le front n'ajoute **jamais**
  d'en-tête `Authorization`. Même origine en développement (relais Vite `/api`) : ni CORS ni
  `credentials` à régler.
- L'API renouvelle elle-même la session : **un `401` veut dire « vraiment déconnecté »**. Le
  front ne rafraîchit rien ; il vide le cache (`queryClient.clear()`) et mène à la connexion
  avec la **page de retour**.
- ⚠ **La page de retour se valide** avant d'y aller : un chemin **relatif** de l'application
  (commence par un seul `/`, ni `//`, ni schéma, ni `\`), sinon l'accueil. Sinon un lien piégé
  (`/connexion?retour=https://ailleurs`) devient une redirection ouverte.
- « Se déconnecter » consulte les gardes de sortie (saisie non enregistrée), appelle l'API,
  vide le cache. Une session fermée ailleurs lève les gardes sans rien demander.
- Mobile : voir `structure.md` (jetons en stockage sécurisé, `Authorization: Bearer`).

### Front et API sur deux sites

Si l'API n'est pas sur le même site que le front (domaines différents), les cookies sont en
`SameSite=None` et l'API exige une protection CSRF : le client envoie ses requêtes avec
`credentials: 'include'` et, si l'API le demande, recopie son jeton anti-CSRF (lu dans le cookie
prévu pour lui) dans l'en-tête convenu, sur **toute** requête qui écrit — une seule fois, dans le
client du paquet `api`, jamais écran par écran. Le relais `/api` du développement ne montre pas
ce cas : l'éprouver sur un environnement à deux domaines.

## Espaces de travail et permissions

- La liste des espaces (`GET /api/me/workspaces`) porte, pour chacun, ses **permissions** ; un
  hook (`useWorkspace()`) lit l'espace de l'adresse et les fournit.
- **Une action s'offre si l'espace a la permission et s'il permet d'écrire** (abonnement en
  lecture seule : `allowsWrite()`, gestes de sécurité gardés offerts). **Jamais d'après un nom
  de rôle.**
- **Les gestes d'une ressource suivent ses drapeaux `can…`** rendus par l'API (plus fins que la
  permission : état de la ressource, portée). Une borne publiée par l'API (dates permises,
  longueur) s'applique au champ (`min`/`max`, raison dans l'aide) ; le `400` reste sous le champ.
- Une lecture qui a **une route par permission** : lire chaque route que l'espace ouvre et
  réunir les drapeaux ; jamais la route d'une permission absente.
- Une portée réduite (membre limité à une partie de l'unité) : l'API filtre ; l'écran affiche
  un rappel de portée.
- Ce qu'une famille de permissions ouvre (menus, onglets, boutons) se décide **en un seul
  fichier** de la fonctionnalité (`access.ts`), que menus, routes et pages appellent.

## Refus de l'API

| Cas | Réaction |
|---|---|
| Page ouverte par son adresse sans la permission | page « accès refusé » dans la coquille |
| `403 FORBIDDEN`, lecture seule, organisation suspendue | `detail` affiché, liste des espaces relue (la permission a pu changer) ; jamais de déconnexion |
| `404` d'une ressource (hors portée ou inconnue) | page « introuvable » intégrée à la coquille, lien de retour à la liste |
| `ROUTE_NOT_FOUND` | erreur de programmation : message générique, trace console |

## Pages publiques

Une page sans compte (vérification d'un document, invitation) vit **hors de la coquille** : ses
seuls appels sont les siens (ni session, ni espaces, ni lookups), `noindex`, rien gardé en cache
(`gcTime: 0`, jamais relue d'elle-même) ; son `429` attend « Réessayer ».
