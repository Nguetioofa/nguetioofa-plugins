# TypeScript, composants et hooks

## TypeScript

`packages/config/tsconfig.json` (étendu par tous : `"extends": "@projet/config/tsconfig.json"`) :

```jsonc
{
  "compilerOptions": {
    "target": "ES2022",
    "lib": ["ES2023", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "moduleResolution": "bundler",
    "jsx": "react-jsx",
    "strict": true,                      // nullabilité en erreur
    "noUncheckedIndexedAccess": true,    // tab[i] peut être undefined : on le traite
    "noImplicitOverride": true,
    "noFallthroughCasesInSwitch": true,
    "verbatimModuleSyntax": true,        // import type { … } pour les types
    "isolatedModules": true,
    "skipLibCheck": true
  }
}
```

- **`any` interdit** (règle ESLint en erreur) ; un cas inévitable porte un commentaire qui
  dit pourquoi. Pour une donnée inconnue : `unknown`, puis vérification.
- **Les types de l'API sont générés** : `import type { components } from '@projet/api'` ou
  les alias exportés par le paquet (`OrderListItem`). On ne les réécrit jamais.
- `interface` pour les propriétés d'un composant et les objets ; `type` pour les unions
  (`type Density = 'comfortable' | 'compact' | 'touch'`). Pas d'`enum` TypeScript : unions de
  chaînes (les énumérations de l'API arrivent déjà en texte).
- **Exports nommés** partout (pas d'`export default`, sauf si un outil l'exige).
- Pas d'assertion `as` pour faire taire le compilateur ; `!` (non-null) seulement quand une
  vérification précède et que le compilateur ne la voit pas — avec un commentaire.

## Composants

```tsx
interface OrderStatusBadgeProps {
  code: OrderStatusCode;        // code venu de l'API
  size?: 'sm' | 'md';
}

export function OrderStatusBadge({ code, size = 'sm' }: OrderStatusBadgeProps) {
  const label = useLookupLabel('orderStatus', code);   // libellé depuis GET /api/lookups
  return <Badge variant={orderStatusVariant[code]} size={size}>{label}</Badge>;
}
```

- **Des fonctions**, un composant par fichier, nommé comme lui ; une ligne JSDoc sur son rôle
  seulement si son nom ne suffit pas.
- **Propriétés** : typées, valeurs par défaut dans la déstructuration, jamais modifiées.
  Callbacks nommés `on<Événement>` (`onOpen`, `onChange`) ; le contenu passe par `children`.
- **Pur** : même props et même état → même rendu ; aucun effet de bord pendant le rendu
  (pas d'appel réseau, pas d'écriture dans le stockage, pas de `Date.now()` qui change le
  rendu sans raison).
- **Petit** : au-delà de ~150 lignes ou de trois responsabilités, extraire un composant ou
  un hook.
- **Composition** plutôt que configuration : `<Card><CardHead/>…</Card>` plutôt qu'un
  composant à trente propriétés.
- **Listes** : `key` = l'identifiant stable (`publicId`), **jamais l'indice**.
- **Formulaires et champs** : contrôlés par React Hook Form (`register`, `Controller` pour
  les composants maison).
- **`ref`** : en React 19, `ref` est une propriété ordinaire d'un composant fonction (plus de
  `forwardRef`).
- **HTML natif d'abord** : `<button>` pour une action, `<a href>` pour une navigation,
  `<label>` pour un champ, `<table>` pour un tableau, `<dialog>` pour une modale.

## Hooks

- **Règle d'or** : un hook s'appelle en haut de la fonction, toujours dans le même ordre —
  jamais dans un `if`, une boucle, un callback, ni après un `return` anticipé.
- **Hook maison** (`useOrderFilters`, `useWorkspace`) : un nom qui commence par `use`, un
  fichier, une responsabilité ; il rend des valeurs et des fonctions, pas du balisage.
- **`useEffect` est rare** : pour se synchroniser avec l'extérieur (abonnement, minuterie,
  focus, `document.title`), toujours avec son nettoyage. **Jamais** pour charger des
  données (→ `useQuery`), ni pour calculer une valeur dérivée (→ variable ou `useMemo`), ni
  pour réagir à un clic (→ le gestionnaire du clic).
- **`useMemo` / `useCallback`** : seulement pour un calcul coûteux mesuré, ou pour stabiliser
  une valeur passée à un composant mémorisé. Pas par réflexe.
- **Contexte** (`createContext` + hook d'accès `useTheme()`) : pour ce qui est transverse
  (thème, langue, utilisateur, espace de travail, permissions) ; valeur mémorisée ; le hook
  d'accès lève une erreur claire hors de son fournisseur.

## Commentaires

Comme au back : **le pourquoi, jamais le quoi**, en français, au plus 1 ligne pour 5 lignes de
code utiles. Une ligne JSDoc sur un composant ou un hook exporté dont le nom ne dit pas le rôle,
souvent aucune ; une propriété commentée seulement si elle n'est pas évidente.
