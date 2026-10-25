# Structure : monorepo, paquets, fonctionnalités

## Le monorepo pnpm

```
frontend/
├─ package.json            scripts communs, "private": true, "packageManager": "pnpm@<version>"
├─ pnpm-workspace.yaml     paquets + catalogue des versions
├─ pnpm-lock.yaml          versionné : versions exactes pour tout le monde
├─ (tsconfig partagé dans packages/config/tsconfig.json, étendu partout)
├─ eslint.config.js        règles communes (configuration « flat »)
├─ .prettierrc             mise en forme
├─ .editorconfig
├─ .nvmrc                  version de Node (24)
├─ apps/web/
└─ packages/<nom>/
```

`pnpm-workspace.yaml` — l'équivalent de la solution **et** de `Directory.Packages.props` :

```yaml
packages:
  - apps/*
  - packages/*

# Versions centralisées : un package.json écrit "react": "catalog:"
catalog:
  react: ^19.0.0
  react-dom: ^19.0.0
  typescript: ^5.9.0
  # …
```

- **Une version par dépendance, dans le catalogue.** Un `package.json` n'écrit jamais de
  numéro de version pour une dépendance du catalogue.
- Les versions ci-dessus sont des exemples : **vérifier la version stable courante** avant
  d'installer, et qu'elle est suivie par l'outillage qui en dépend (ESLint et ses plugins,
  générateur du client, Vitest) ; une version refusée pour cette raison se note, avec sa
  raison, dans le `CLAUDE.md` du projet.
- Dépendance entre paquets : `"@projet/ui-web": "workspace:*"`.
- **Pas d'orchestrateur** (Turborepo, Nx) tant que `pnpm -r` suffit : les scripts racine
  lancent `pnpm -r --parallel dev`, `pnpm -r build`, `pnpm -r test`.
- Node fixé par `.nvmrc` et `"engines"` ; pnpm fixé par `"packageManager"` (activé par
  `corepack enable`).

## Les paquets

| Paquet | Contient | Dépend de | Ne contient jamais |
|---|---|---|---|
| `tokens` | `tokens.css` (rôles clair et sombre), `index.ts` (noms des rôles, script de thème) | — | composant, logique |
| `ui-core` | hooks sans rendu (`useSelect`, `useDataTableQuery`, `useDisclosure`), formats `Intl` (montant, date, téléphone, pluriels), calculs purs | `tokens` (types) | DOM, CSS, appel réseau |
| `ui-web` | composants React DOM maison, leurs CSS (classes au préfixe court du projet, `<p>-*`) | `ui-core`, `tokens` | appel réseau, texte métier, logique métier |
| `api` | client généré (`schema.d.ts`), `createApiClient`, lecture de l'enveloppe, `ApiError` typée par `errorCode`, clés de requête | — | composant |
| `i18n` | configuration i18next, langues, textes communs (`common`, `errors`) fr / en, table des libellés techniques des composants | — | textes d'une fonctionnalité |
| `config` | `tsconfig` et règles ESLint partagées | — | code |

- **Niveau léger** (outil interne) : seulement `apps/web` et `packages/api` au départ ; tokens,
  textes et composants vivent dans `apps/web/src/shared/` et passent en paquet le jour où un
  deuxième consommateur réel arrive.
- Nom : `@<projet>/<paquet>` (`@projet/ui-web`).
- Chaque paquet expose son contenu par `package.json` → `"exports"` et un `src/index.ts`
  qui **liste** ce qui est public ; on n'importe jamais un fichier interne d'un paquet
  (`@projet/ui-web/src/Button/…` interdit).
- Les paquets sont consommés **en source TypeScript** par Vite (pas de compilation séparée
  des paquets tant qu'il n'y a qu'une application) : plus simple, rechargement à chaud
  partout.
- Un paquet **mobile** (`ui-native`) ne se crée que le jour où l'application mobile existe
  (`apps/mobile`, Expo) : elle reprend `api`, `i18n`, `ui-core` et `tokens` (d'où leur
  interdiction du DOM), garde ses jetons dans un stockage sécurisé (Keychain / Keystore) et
  envoie `Authorization: Bearer` — le contraire du web, qui ne voit aucun jeton.

## L'application

```
apps/web/
├─ index.html              script de thème AVANT les styles, <html lang>
├─ vite.config.ts          relais /api vers l'API en développement
├─ public/                 favicon, police auto-hébergée
└─ src/
   ├─ main.tsx             monte l'application : <AppProviders><RouterProvider/></AppProviders>
   ├─ app/
   │  ├─ AppProviders.tsx  QueryClientProvider, I18nextProvider, ThemeProvider, AuthProvider…
   │  ├─ router.tsx        createBrowserRouter : coquille + routes de chaque fonctionnalité
   │  ├─ shell/            coquille : barre latérale, barre supérieure, fil d'Ariane
   │  └─ errors/           pages 404, erreur inattendue (traceId copiable)
   ├─ features/
   │  └─ <feature>/        une fonctionnalité métier (voir ci-dessous)
   └─ shared/              hooks, composants, utilitaires servant à plusieurs fonctionnalités
```

## Une fonctionnalité

```
features/orders/
├─ routes.tsx          adresses, chargées à la demande (lazy)
├─ api.ts              requêtes : useOrders, useOrder, useCreateOrder…
├─ pages/              OrderListPage.tsx (+ .module.css de mise en page)
├─ components/         OrderCell.tsx, OrderStatusBadge.tsx…
├─ hooks/              useOrderFilters.ts (état d'adresse de la liste)…
└─ i18n/               fr.json, en.json (espace de noms « orders »)
```

- Nommage : dossier de fonctionnalité en **anglais**, au **pluriel** pour une ressource
  (`orders`, `payments`), au singulier pour un domaine (`auth`, `account`).
- Fichiers de composants en `PascalCase.tsx`, un composant par fichier, nommé comme lui ;
  hooks en `useCamelCase.ts` ; le reste en `camelCase.ts`.
- **Une fonctionnalité n'importe jamais une autre fonctionnalité.** Ce dont deux
  fonctionnalités ont besoin monte dans `shared/` ; ce qui est générique (un bouton, une
  liste de choix) va dans `ui-web`.
- Un dossier vide ne se crée pas « pour plus tard ».

## Ajouter…

- **une fonctionnalité** : le dossier, son `routes.tsx` branché dans `app/router.tsx`, ses
  espaces de noms de traduction déclarés, la description des routes qu'elle consomme lue ;
- **un composant générique** : dans `ui-web`, selon la spécification du système d'UI, avec
  sa démonstration et ses états ;
- **un paquet** : seulement s'il est réutilisé par deux consommateurs **réels** ; le
  proposer d'abord.
