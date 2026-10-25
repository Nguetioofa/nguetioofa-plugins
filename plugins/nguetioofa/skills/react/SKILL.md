---
name: react
description: Méthode, philosophie et conventions de Nguetioofa pour tout front React + TypeScript (monorepo pnpm, Vite, React Router, TanStack Query, client d'API généré depuis l'OpenAPI, React Hook Form, i18next, thèmes clair et sombre, composants maison, Vitest). À utiliser dès qu'on crée, modifie, vérifie ou relit un front web de ses projets (un front mobile en reprend la méthode, voir structure.md) — squelette, paquet, page, composant, hook, requête, formulaire, route, traduction, style, thème, test — même si l'utilisateur ne nomme pas le skill. Le système d'UI propre au projet (son skill d'UI, s'il en a un) s'y ajoute et l'emporte pour l'apparence.
---

# Fronts React + TypeScript — la méthode Nguetioofa

Ce skill décrit **comment on travaille** sur un front React : rangement, TypeScript, état,
données, traduction, thème, styles, vérification. Il est le pendant de
skill `dotnet` du même plugin (même rigueur, même manière de travailler), vaut pour tous les fronts
de Nguetioofa et se suffit à lui-même : ses fiches portent les modèles d'un projet neuf.

- **Avant d'écrire un fichier d'un type donné, relire son équivalent dans le projet en
  cours** (s'il en a déjà un) et l'imiter.
- **Le `CLAUDE.md` du projet l'emporte** sur ce skill quand ils diffèrent.
- **Le système d'UI du projet** (son skill d'UI, s'il en a un) l'emporte pour tout ce qui se
  voit : couleurs, composants, écrans, textes. Sans lui, les règles de
  [`composants-et-styles.md`](references/composants-et-styles.md) suffisent.
- **L'utilisateur vient de Blazor** (et connaît JS, HTML, CSS) : chaque explication rattache
  la notion React à son équivalent Blazor — voir
  [`references/blazor-vers-react.md`](references/blazor-vers-react.md).

---

## 1. La philosophie

1. **Lisible avant tout.** Même rigueur que le back, sans architecture futuriste : des
   dossiers par fonctionnalité, des noms clairs, pas de couche « au cas où ».
2. **Construire ce qui existe, pas ce qui viendra peut-être.** Une seule application fait
   tout ; on ne sépare en paquets que ce qui **sera** réutilisé (composants, logique, client
   d'API, tokens).
3. **L'API fait foi.** Les types viennent du client **généré** depuis son document OpenAPI ;
   un besoin non couvert se demande au backend (par le moyen que le projet a fixé), il ne
   s'invente pas.
4. **Trois sources d'état, chacune à sa place** : l'**API** (TanStack Query), l'**adresse**
   (espace, filtres, tri, page), le **composant** (`useState`). Jamais de copie de l'une dans
   l'autre, pas de magasin global.
5. **Un composant affiche.** La logique va dans un hook, le calcul pur dans une bibliothèque
   testée, l'appel réseau dans le fichier `api.ts` de la fonctionnalité.
6. **Traduit et thémé dès la première ligne.** Aucun texte en dur (fr **et** en), aucune
   couleur en dur (rôles de tokens), clair **et** sombre.
7. **Accessible par construction** : HTML natif d'abord, clavier, focus visible, ARIA juste,
   contrastes mesurés.
8. **Une erreur de l'API a une réaction prévue**, décidée par son `errorCode`, jamais par son
   message.
9. **Explicite plutôt que magique** : exports nommés, types écrits, dépendances proposées
   avant d'être ajoutées.
10. **Vérifier dans le navigateur avant d'affirmer** : compiler ne prouve rien pour une
    interface.
11. **Commenter seulement quand c'est vraiment nécessaire : le pourquoi non évident, jamais le
    quoi.** **Au plus 1 ligne de commentaire pour 5 lignes de code utiles**, dans chaque fichier
    (JSDoc compris) — la même règle qu'au back. Un type, une petite fonction ou un composant au nom clair : **une ligne au
    plus**, souvent aucune. Jamais de paraphrase, d'historique (dates, décisions, étapes), ni de
    règle déjà écrite ailleurs. **Jamais de « avant on faisait…, maintenant on fait… »** (ni
    « désormais », « ne … plus », « remplace l'ancien ») — dans le code comme dans la
    documentation : on décrit ce qui est ; l'ancien état est dans l'historique git.
12. **Un test seulement pour ce qui vaut vraiment la peine** : une logique pure qu'une
    régression casserait sans bruit (calcul, format, état d'un assistant), une règle de droits
    ou de données sensibles. Jamais pour tout et n'importe quoi (composant qui affiche, hook
    trivial, chaque variante d'un même cas). Peu de tests, mais chacun avec sa contre-épreuve.

## 2. La manière de travailler

- **Le français partout** ; identifiants de code en anglais.
- **Ne coder que ce qui est demandé.** Une question de cadrage n'est pas un feu vert.
  **Par morceaux validés** : annoncer le découpage, livrer un morceau vérifié, attendre la
  validation.
- **Expliquer pour un développeur Blazor**, dans les réponses adressées à l'utilisateur
  seulement (pas dans le code, ni dans le rapport d'un agent au coordinateur) : à une notion
  qu'il découvre (hook, contexte, props, rendu), une phrase et son équivalent Blazor.
- **Poser la question quand la réponse change ce qu'on fait** (règle métier, dépendance,
  parcours) ; sinon prendre le défaut de ce skill et le dire.
- **Git : l'IA ne committe jamais le code**, n'indexe rien, ne pousse rien ; elle liste les
  fichiers à committer. Un projet peut écrire une exception dans son `CLAUDE.md` (ex. un
  dépôt que l'IA committe elle-même).
- **Aucune dépendance sans proposition** : son rôle, pourquoi pas sans elle, son poids.
- **Aucun secret dans un front** : tout ce qui part dans le navigateur est public.
- **Dire la vérité sur ce qui a été vérifié** (navigateur, thèmes, langues, clavier), et ce
  qui ne l'a pas été.

## 3. Où mettre quoi

| Quoi | Où |
|---|---|
| Liste des paquets, catalogue des versions | `pnpm-workspace.yaml` |
| Scripts communs (`dev`, `build`, `test`, `lint`, `typecheck`, `gen:api`) | `package.json` racine |
| L'application | `apps/web/` (`index.html`, `src/main.tsx`, `src/app/`, `src/features/`, `src/shared/`) |
| Démarrage, coquille, routes, fournisseurs (thème, langue, API, espace) | `apps/web/src/app/` |
| Une fonctionnalité | `apps/web/src/features/<feature>/` : `routes.tsx`, `api.ts`, `pages/`, `components/`, `hooks/`, `i18n/fr.json` + `en.json` |
| Partagé entre fonctionnalités de l'application | `apps/web/src/shared/` |
| Tokens (couleurs par rôle, clair et sombre) | `packages/tokens/` |
| Logique sans rendu, formats `Intl`, calculs purs | `packages/ui-core/` |
| Composants maison | `packages/ui-web/` |
| Client d'API généré + lecture de l'enveloppe + erreurs typées | `packages/api/` |
| Configuration des traductions + textes communs | `packages/i18n/` |
| Réglages TypeScript et ESLint partagés | `packages/config/` |
| Guides pour humains | `docs/` |

Sens des dépendances : `apps` → `packages` ; `ui-web` → `ui-core` → `tokens` ; une
fonctionnalité n'importe jamais une autre fonctionnalité. Détail :
[`references/structure.md`](references/structure.md).

## 4. Déroulé d'une livraison (une page, une fonctionnalité)

1. **Cadrer** : `CLAUDE.md` du projet, la description des routes que l'écran consomme (là où le
   projet la tient), patron d'écran du système d'UI. Proposer le découpage.
2. **Client d'API** : `pnpm gen:api` si le document OpenAPI a changé ; les types sont là. Une
   erreur de compilation après régénération est un changement cassant à traiter, pas à
   contourner.
3. **Données** : les hooks de `api.ts` (lecture `useQuery`, écriture `useMutation`,
   invalidation) → [`references/etat-et-donnees.md`](references/etat-et-donnees.md).
4. **Adresses** : `routes.tsx` (chargement à la demande), état d'adresse (filtres, tri, page).
5. **Écran** : page et composants, uniquement avec les composants maison et les tokens →
   [`references/composants-et-styles.md`](references/composants-et-styles.md). Une page de
   modification, un geste, un lot → [`references/gestes-et-formulaires.md`](references/gestes-et-formulaires.md) ;
   ce qui s'offre selon les droits → [`references/droits-et-session.md`](references/droits-et-session.md) ;
   une donnée personnelle → [`references/donnees-sensibles-et-securite.md`](references/donnees-sensibles-et-securite.md).
6. **Textes** : clés fr + en dans le même geste ; formats `Intl` →
   [`references/i18n-et-theme.md`](references/i18n-et-theme.md).
7. **Vérifier** dans le navigateur : parcours complet, chaque erreur, clair et sombre, fr et
   en, clavier, 360 px, console et requêtes propres ; tests des règles qui le méritent →
   [`references/verification-et-tests.md`](references/verification-et-tests.md).
8. **Rendre compte** : ce qui est fait et prouvé, fichiers à committer, ce qui reste à demander
   au backend.

Relire [`references/pieges.md`](references/pieges.md) avant de conclure.

## 5. Fiches de référence

| Fiche | La lire quand |
|---|---|
| [`structure.md`](references/structure.md) | on crée le squelette, un paquet, une fonctionnalité, une route |
| [`typescript-et-composants.md`](references/typescript-et-composants.md) | on écrit un composant, un hook, un type |
| [`etat-et-donnees.md`](references/etat-et-donnees.md) | on lit ou écrit des données, on gère l'adresse, un formulaire, une erreur de l'API |
| [`gestes-et-formulaires.md`](references/gestes-et-formulaires.md) | page de modification, double envoi, conflit de version, garde de sortie, lot, assistant, nouvelle tentative, fichier |
| [`droits-et-session.md`](references/droits-et-session.md) | connexion et déconnexion, page de retour, espaces, permissions, drapeaux `can…`, refus `401` / `403` / `404`, page publique |
| [`donnees-sensibles-et-securite.md`](references/donnees-sensibles-et-securite.md) | une donnée personnelle ou sensible, une recherche sur une personne, un lien, du HTML, le stockage local |
| [`production.md`](references/production.md) | build, déploiement, cache, en-têtes de sécurité, barrière d'erreur, impression, accessibilité outillée |
| [`composants-et-styles.md`](references/composants-et-styles.md) | on construit un composant de bibliothèque ou on met en page un écran |
| [`i18n-et-theme.md`](references/i18n-et-theme.md) | textes, langues, formats, thème clair/sombre, densité, responsive |
| [`verification-et-tests.md`](references/verification-et-tests.md) | on vérifie, on écrit des tests |
| [`blazor-vers-react.md`](references/blazor-vers-react.md) | on explique une notion à l'utilisateur |
| [`pieges.md`](references/pieges.md) | toujours, avant de conclure |
