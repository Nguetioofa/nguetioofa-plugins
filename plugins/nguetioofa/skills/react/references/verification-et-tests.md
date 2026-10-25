# Vérification et tests

**Rien ne se déclare fait sur la foi d'une compilation.** Une interface se vérifie en la
faisant tourner et en la regardant.

## Vérifier une livraison

1. `pnpm typecheck` et `pnpm lint` propres (aucun avertissement laissé sans raison).
2. **Lancer** l'API puis `pnpm dev` ; ouvrir la page dans le navigateur (outil de
   prévisualisation de la session, configuré dans `.claude/launch.json`, ignoré par git).
3. **Parcourir** le chemin principal **et chaque erreur** que l'API documente pour ces routes (validation,
   introuvable, conflit, droits, plafond 429, panne) : le bon message, au bon endroit.
4. **Console** : aucune erreur ni avertissement React (clé manquante, hook mal placé).
   **Requêtes** : la bonne route, l'espace de travail dans l'adresse, `Accept-Language`,
   pas de requête en double ni de requête orpheline après un changement de page.
5. **Clair et sombre**, **fr et en**, **clavier seul** (ordre, focus visible, `Échap`),
   **1440 / 1024 / 360 px**, densité tactile.
6. **États** : chargement, vide, erreur avec « Réessayer ».
7. **Contre quelle API** : l'API réelle quand elle a la version que l'écran attend (lire
   `info.version` de son document Swagger) ; sinon une API d'essai à part (autre port, base
   d'essai) servie sur un **autre nom d'hôte** (`verif.localhost`) pour ne pas partager les
   cookies de `localhost`, ou des réponses simulées — et le dire.
8. Rendre compte avec les preuves (ce qui a été cliqué, ce qui a été vu) et ce qui n'a pas pu
   l'être.

## Tests automatiques

**Pour ce qui en vaut la peine, pas pour tout.** Méritent un test :

- la **logique pure** de `ui-core` : formats (montants, dates et fuseaux, téléphone), filtre sans
  accents, cycle de tri, sélection « tous sauf… », délai et annulation de recherche ;
- la **lecture de l'enveloppe** et le classement des erreurs (`api`) ;
- le **comportement clavier et ARIA** des composants maison complexes (Select, DataTable,
  Menu, Dialog) ;
- la **complétude des traductions** (mêmes clés en fr et en) ;
- une règle d'écran qu'une régression casserait sans bruit (masquer une action selon les
  permissions).

Ne méritent pas de test : une page qui assemble, un composant qui affiche une prop. En
conversation avec l'utilisateur, proposer les tests avant de les écrire ; un agent qui
travaille sur consigne écrit seulement ceux qui le méritent.

Outils et règles :

- **Vitest** (même configuration que Vite) + **Testing Library** : on cherche les éléments
  **comme un utilisateur** (`getByRole('button', { name: 'Encaisser' })`), jamais par classe
  CSS ni par identifiant technique.
- Noms de tests **en français**, qui disent la règle : `it('trouve « Éloïse » en tapant « eloise »')`.
- Réseau simulé au niveau du `fetch` (réponses d'enveloppe réalistes, dont les erreurs),
  jamais un vrai appel.
- **Contre-épreuve** : casser la règle, voir le test échouer, restaurer.
## Tests de bout en bout (Playwright)

- **Dès le premier parcours critique**, pas « plus tard » : la connexion et la déconnexion, puis
  un parcours par fonctionnalité sensible (droits, argent, données personnelles), contre une API
  d'essai et une base d'essai, jamais celles de l'utilisateur.
- Chercher les éléments par rôle et nom, comme dans Testing Library ; une session par personne
  de test (`storageState`) ; vérifier en fr et en en au moins une fois par parcours.
- Peu nombreux et stables : un test qui échoue au hasard se répare ou se retire, il ne
  s'ignore pas.

## Intégration continue

**À chaque push** : `pnpm install --frozen-lockfile`, `pnpm typecheck`, `pnpm lint`,
`pnpm test`, le build, les tests Playwright (avec l'API et la base d'essai du pipeline), et
`pnpm audit --prod` (une faille haute ou critique fait échouer le pipeline). Écrit au squelette du
projet, pas à la fin.

## La taille des fichiers

La règle d'ESLint `max-lines` (400 lignes, lignes vides et commentaires exclus) est **en erreur**
pour le code (pas pour les tests ni le client généré) ; une exception se déclare dans le fichier,
avec sa raison. Un composant de plus de ~150 lignes ou de trois responsabilités se découpe.
