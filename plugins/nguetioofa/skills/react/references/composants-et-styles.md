# Composants maison et styles

Le **système d'UI du projet** (son skill d'UI, s'il en a un) fixe l'apparence, les
composants et leur comportement. Ce document dit **comment on les écrit** en React ; sans
système d'UI, ses règles et celles de [`i18n-et-theme.md`](i18n-et-theme.md) suffisent à
en bâtir un.

## La bibliothèque maison

- **Pas de bibliothèque de composants stylés** (pas de MUI, Ant, Chakra…) : l'apparence est
  celle du système d'UI. Mais **le comportement des composants difficiles vient d'une
  bibliothèque sans style reconnue** — liste de choix et saisie assistée, sélecteur de dates,
  menu, onglets, fenêtre, info-bulle, curseur : React Aria Components (`react-aria-components`)
  ou Radix Primitives, une seule des deux par projet. Le clavier, le focus, l'ARIA et les
  lecteurs d'écran y sont déjà justes ; le composant maison les habille par les tokens et fixe
  son API. Vérifier la version courante et son poids avant de l'ajouter (proposition à
  l'utilisateur).
- Écrits entièrement à la main : les composants simples (bouton, champ, badge, carte) et ce
  qu'aucune bibliothèque ne fait comme le projet le veut, à condition de le dire.
- Utilitaires **sans rendu** admis : `@floating-ui/react-dom` (positionner), `@tanstack/react-table`
  et `@tanstack/react-virtual` (tableaux et longues listes), `lucide-react` (icônes). Une
  modale simple peut rester un `<dialog>` natif (`showModal()` donne le piège du focus, `Échap`
  et l'arrière-plan inerte).
- **Logique dans `ui-core`, rendu dans `ui-web`** : `useSelect` (filtre local sans accents,
  recherche distante avec délai et annulation, sélection multiple, clavier) est partageable
  avec un futur rendu mobile ; `<Select>` (DOM + classes) ne l'est pas.
- **Même API d'un rendu à l'autre** : propriétés au vocabulaire ARIA (`disabled`, `invalid`,
  `busy`, `selected`), textes reçus déjà traduits, libellés techniques (« Fermer », « Aucun
  résultat ») par une table fournie par l'application.
- Un composant **ne connaît pas l'API** : il reçoit `loadOptions`, `fetchPage`, `onChange`.
- Contrôlé par défaut (`value` + `onChange`).
- **Fini** = spécifié dans le système d'UI, logique testée, démontré avec **tous** ses états
  (vide, chargement, erreur, désactivé, invalide, texte long), vérifié en clair et sombre,
  densités, 360 px, clavier, lecteur d'écran, fr et en.

Un composant, un dossier :

```
packages/ui-web/src/Select/
├─ Select.tsx
├─ select.css          ses règles, dans @layer components, rôles de tokens uniquement
├─ Select.test.tsx
└─ index.ts
```

## Les styles

- **CSS natif**, sans préprocesseur, sans Tailwind, sans CSS-in-JS.
- **Couches** : `@layer tokens, base, components, utilities` — une règle de page ne gagne
  jamais par accident sur un composant.
- Couleurs, espacements, tailles, rayons, durées : **uniquement des rôles**
  (`var(--color-fg-muted)`, `var(--space-4)`) — jamais un hex, jamais une primitive
  (`--blue-600`), jamais un `px` libre (sauf `1px` de bordure).
- **État par attribut**, pas par classe : `:disabled`, `[aria-invalid="true"]`,
  `[aria-selected="true"]`, `[aria-current="page"]`, `[aria-expanded="true"]`,
  `[aria-busy="true"]`.
- **Mise en page d'une page** : un CSS Module à côté d'elle (`OrderListPage.module.css`,
  `className={styles.layout}`), rôles de tokens uniquement, **aucune règle de composant**
  dedans (on ne restyle pas un bouton dans une page).
- `style={{ … }}` seulement pour une valeur **calculée** (largeur d'une barre de progression).
- **Responsive** : grilles `auto-fit, minmax()`, `flex-wrap`, points de rupture du système
  d'UI (640 / 1024 / 1280) ; priorité au bureau pour l'administration, mais **tout reste
  utilisable à 360 px** (le tableau devient une liste de cartes, la barre latérale un tiroir).
- Police auto-hébergée (sous-ensembles latin et latin-ext, `font-display: swap`).

## Icônes

`lucide-react`, trait 2 px, `currentColor`, tailles du système d'UI (16 / 20 / 24). Décorative
→ `aria-hidden`. Seule dans un bouton → le bouton a un `aria-label` précis. Le dictionnaire
concept → icône est dans le système d'UI : on n'improvise pas une icône.
