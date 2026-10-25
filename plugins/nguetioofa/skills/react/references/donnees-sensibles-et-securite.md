# Données sensibles et sécurité du front

Tout ce qui part dans le navigateur est public, et tout ce qui y reste peut fuir (historique,
cache, console, extensions, poste partagé). Le front ne protège pas les données à la place de
l'API ; il évite seulement de les répandre.

---

## Rien de personnel dans l'adresse

- Une recherche par nom, par numéro ou par email reste dans la page et part dans le corps d'un
  `POST` de recherche (lu par `useQuery`). Une adresse finit dans l'historique, les journaux et
  l'en-tête `Referer`.
- D'une page à l'autre, l'historique (`navigate(…, { state })`) ne porte qu'un **identifiant
  opaque** ; le reste passe en mémoire (perdu au rechargement : la page le dit).
- Un motif, une remarque, un texte libre sur une personne : dans le corps de la demande et le
  détail lu à l'ouverture, jamais dans une clé de cache, l'adresse, la console ni un stockage.

## Données sensibles en cache

Une donnée sensible (santé, pièce d'identité, données d'un mineur, fichier importé, données
d'une autre unité) :

- lue **à la demande**, `gcTime: 0`, **sans nouvelle tentative** ;
- **jamais** dans `localStorage` / `sessionStorage` / IndexedDB, l'adresse, l'historique ou
  la console ;
- sous une **racine de clé à part**, qu'aucune invalidation large ne relit sans le vouloir
  (une lecture qui date « vu le… » côté API ne doit pas être déclenchée par une relecture de
  fond).
- Le cache entier se vide à la déconnexion.
- Une image sensible (signature, cachet) : relue à chaque ouverture, rendue en adresse
  `data:` plutôt que `blob:` (une adresse `blob:` reste lisible tant que la page vit), sans
  cache.

## Injection et liens

- **Jamais `dangerouslySetInnerHTML`** avec un contenu venu de l'API ou de la personne ; un texte
  riche (éditeur) se rend par sa bibliothèque, qui assainit.
- Un lien vers un autre site : `target="_blank"` **avec** `rel="noopener noreferrer"`.
- Une adresse construite à partir d'une donnée (`href`) n'accepte que `http:`, `https:`,
  `mailto:`, `tel:` (jamais `javascript:`).
- Les redirections après connexion se valident (`droits-et-session.md`).

## Stockage local

- Admis seulement pour des **préférences** (langue, thème, densité, disposition d'un tableau,
  dernier espace utilisé) — jamais une donnée métier ni un jeton.
- Toute lecture d'un stockage se fait dans un `try` : il peut être absent ou plein.

## Secrets

- **Aucun secret dans un front** : une variable `VITE_…` est **publique** (elle est écrite dans
  le paquet servi). Une clé d'API tierce vit côté serveur.
