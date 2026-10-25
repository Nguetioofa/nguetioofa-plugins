# Build, déploiement, robustesse

Ce qui sépare un front qui marche sur `pnpm dev` d'un front servi à de vraies personnes.

---

## Build

- `pnpm build` (Vite) produit des fichiers statiques. Pour un build de contrôle pendant le
  travail, l'écrire **hors du dépôt** (`vite build --outDir <dossier de travail> --emptyOutDir`) :
  un `dist/` dans le dépôt se commit par erreur.
- **Variables d'environnement** : `import.meta.env.VITE_…`, lues **au build** et **publiques**
  (`donnees-sensibles-et-securite.md`). L'adresse de l'API en production est la même origine
  (`/api` relayé par le serveur), donc rien à configurer côté front dans le cas courant.
- **Découpage** : chaque fonctionnalité se charge à la demande (`lazy` dans `routes.tsx`) ; une
  page lourde (éditeur, graphiques) charge ses textes et ses bibliothèques avec elle. Un paquet
  d'écrans déclare `"sideEffects"` dans son `package.json` (sinon son index tire tout dans le
  paquet principal). Regarder la taille des paquets produits à chaque ajout d'une dépendance.

## Servir

- Derrière nginx (ou équivalent) : **repli de l'application** (`try_files $uri /index.html`)
  pour que les adresses profondes s'ouvrent ; `/api` relayé vers l'API (même origine : cookies
  `SameSite=Strict`, pas de CORS).
- **Cache** : fichiers à empreinte (`assets/*-[hash].js`) en `Cache-Control: public,
  max-age=31536000, immutable` ; **`index.html` jamais en cache** (`no-cache`), sinon une
  nouvelle version n'arrive pas.
- En-têtes de sécurité posés par le serveur : `Content-Security-Policy` (scripts de la même
  origine ; le script de thème inline dans `index.html` se déclare par son empreinte),
  `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`,
  `frame-ancestors 'none'` sauf besoin.
- Une **nouvelle version** déployée pendant qu'une page est ouverte : un import à la demande qui
  échoue (fichier à empreinte disparu) se rattrape par un rechargement proposé, pas par un écran
  blanc.

## Robustesse

- **Une barrière d'erreur** (error boundary) autour de la coquille et une par page : une page
  qui casse affiche l'écran d'erreur (avec le `traceId` s'il y en a un, copiable), pas un écran
  blanc ; la coquille reste.
- Pages d'erreur prévues : 404 de l'application, accès refusé, erreur inattendue, hors ligne.
- Mode strict de React en développement (il révèle les effets mal nettoyés).

## Impression

- Une page imprimable a sa feuille `@media print` : coquille masquée, **thème clair** forcé
  (le script de thème le fait sur `beforeprint`), couleurs et ombres réduites, sauts de page
  maîtrisés (`break-inside: avoid` sur une ligne, un bloc).
- Un document officiel (PDF produit par l'API) s'imprime depuis le PDF, jamais en imprimant la
  page qui l'affiche.

## Accessibilité outillée

- ESLint `jsx-a11y` en erreur ; les tests de composants cherchent par rôle et nom
  (`getByRole`) : un composant que le test ne trouve pas ainsi n'est pas accessible.
- Un contrôle automatique (axe, par exemple `vitest-axe` ou l'extension du navigateur) sur les
  composants complexes et les pages clés ; il ne remplace pas l'essai au clavier et au lecteur
  d'écran.
