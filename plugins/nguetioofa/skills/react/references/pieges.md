# Pièges

Presque tous **compilent** et ne se voient qu'en faisant tourner l'interface.

## React

| Piège | Symptôme | Correct |
|---|---|---|
| Charger des données dans `useEffect` | requêtes en double (mode strict), réponses dans le désordre, pas d'annulation | `useQuery` avec `signal` |
| Recopier une réponse de l'API dans `useState` | écran périmé après une modification | lire la requête ; invalider après écriture |
| Calcul dérivé dans un `useEffect` + `setState` | rendu en deux temps, boucle possible | variable calculée pendant le rendu (ou `useMemo`) |
| Hook dans un `if` ou après un `return` | « Rendered more hooks than… » | hooks en tête, toujours appelés |
| `key={index}` dans une liste | lignes qui gardent l'état de la voisine après un tri | `key={item.publicId}` |
| Modifier un tableau ou un objet d'état (`push`, `obj.x = …`) | l'écran ne se met pas à jour | nouvelle valeur (`[...liste, x]`, `{ ...obj, x }`) |
| Fermeture périmée (valeur lue dans un callback ancien) | le callback voit une vieille valeur | dépendances complètes (règle ESLint `react-hooks`) ou mise à jour fonctionnelle `setX((x) => …)` |
| Effet sans nettoyage | abonnements ou minuteries qui s'accumulent | renvoyer la fonction de nettoyage |
| `useMemo` / `useCallback` partout | code illisible, aucun gain | seulement sur mesure |
| Effets exécutés deux fois en développement | « bug » qui n'en est pas un | c'est le mode strict : rendre l'effet idempotent |

## Données et adresses

| Piège | Symptôme | Correct |
|---|---|---|
| Clé de requête sans l'espace de travail | les données d'un espace s'affichent dans l'autre | clés qui commencent par l'espace (`['units', id, …]`) |
| Filtres gardés en mémoire seulement | retour arrière ou lien partagé : liste différente | chaîne de requête de l'adresse |
| Tester le message d'une erreur | casse au changement de langue | tester `errorCode` |
| Nouvelle tentative automatique sur un 4xx | trois 409 d'affilée, compteur d'essais consommé | pas de nouvelle tentative sur 4xx |
| Date ISO découpée à la main, `new Date('2008-03-14')` affichée en local | la veille à l'écran selon le fuseau | `Intl.DateTimeFormat` + `timeZone` ; dates sans heure non converties |
| `Authorization` ajouté sur le web | incohérent avec les cookies `HttpOnly` | rien : le navigateur envoie les cookies |
| Appeler l'API sur son adresse (`http://localhost:<port>`) directement en dev | CORS, cookies non envoyés | relais Vite `/api` (même origine) |

## Textes, thème, accessibilité

| Piège | Symptôme | Correct |
|---|---|---|
| Texte dans le JSX, un `aria-label`, un `placeholder` | interface à moitié traduite | `t('…')` partout |
| Clé ajoutée en fr seulement | la clé brute s'affiche en anglais | fr et en dans le même geste, test de complétude |
| Hex ou primitive dans un CSS | faux en thème sombre | rôles de tokens |
| Pas de script de thème avant les styles | éclair blanc au chargement en thème sombre | script dans `index.html` |
| `<div onClick>` comme bouton | ni clavier ni lecteur d'écran | `<button>` |
| Icône seule sans `aria-label` | bouton muet au lecteur d'écran | `aria-label` précis |
| Focus perdu à la fermeture d'une modale | l'utilisateur au clavier repart du haut | rendre le focus au déclencheur (`<dialog>` le fait) |

## Outillage

| Piège | Symptôme | Correct |
|---|---|---|
| Version d'une dépendance écrite dans un `package.json` | deux versions de React dans le monorepo | catalogue de `pnpm-workspace.yaml` |
| Import d'un fichier interne d'un paquet | casse au moindre déplacement | importer depuis l'entrée du paquet |
| `npm install` au lieu de `pnpm` | `package-lock.json` parasite, dépendances incohérentes | toujours `pnpm` |
| Mauvaise version de Node | erreurs d'installation incompréhensibles | `.nvmrc` + `engines` |
| Client d'API non régénéré après un changement de l'API | types faux, erreurs à l'exécution | `pnpm gen:api` à chaque changement du document OpenAPI |
