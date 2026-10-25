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
- **Playwright** (bout en bout) plus tard, pour les parcours critiques (connexion,
  encaissement).
