# État, données de l'API, adresses, formulaires

## Les trois sources d'état

| Source | Outil | Exemples | Jamais |
|---|---|---|---|
| **L'API** | TanStack Query (`useQuery`, `useMutation`) | liste de commandes, fiche, lookups, permissions | recopiée dans un `useState` |
| **L'adresse** | React Router (`useParams`, `useSearchParams`) | espace de travail, période choisie, filtres, tri, page, aperçu ouvert | gardée seulement en mémoire |
| **Le composant** | `useState` | menu ouvert, saisie en cours, onglet d'un panneau | partagée par contexte sans raison |

Transverse à l'application (thème, langue, utilisateur, permissions de l'espace) : un
**contexte** fourni par `app/AppProviders.tsx`. Pas de Redux, pas de Zustand.

## Le client d'API

- Généré par **openapi-typescript** depuis le document OpenAPI de l'API (`pnpm gen:api` →
  `packages/api/src/schema.d.ts`, versionné) ; appelé avec **openapi-fetch** :

```ts
// packages/api/src/client.ts
export const api = createClient<paths>({ baseUrl: '' });   // même origine : Vite relaie /api en dev
api.use({ onRequest: ({ request }) => { request.headers.set('Accept-Language', currentLanguage()); } });
```

- **L'enveloppe se lit une seule fois**, dans le paquet :

```ts
/** Rend data si success, sinon lève une ApiError typée par son errorCode. */
export async function unwrap<T>(call: Promise<{ data?: ApiResponse<T>; error?: ApiResponse<unknown>; response: Response }>): Promise<T>
```

  `ApiError` porte `errorCode`, `status`, `detail` (traduit), `validationErrors`, `traceId`,
  et `retryAfter` (429). Un code inconnu se classe par son statut.
- **Réactions globales** (dans `AppProviders`, une fois) : 401 → effacer le cache et aller à
  la connexion avec la page de retour ; 500 → écran d'erreur avec `traceId` copiable. Les
  autres codes sont traités **là où ils ont un sens** (formulaire, liste).

## TanStack Query

```ts
// features/orders/api.ts
export const orderKeys = {
  all: (unitId: string) => ['units', unitId, 'orders'] as const,
  list: (unitId: string, query: OrderQuery) => [...orderKeys.all(unitId), 'list', query] as const,
  detail: (unitId: string, id: string) => [...orderKeys.all(unitId), 'detail', id] as const,
};

export function useOrders(unitId: string, query: OrderQuery) {
  return useQuery({
    queryKey: orderKeys.list(unitId, query),
    queryFn: ({ signal }) => unwrap(api.GET('/api/units/{unitPublicId}/orders', {
      params: { path: { unitPublicId: unitId }, query: toApiQuery(query) }, signal })),
    placeholderData: keepPreviousData,   // la page précédente reste affichée pendant le chargement
  });
}
```

- **Clés de requête** : une fabrique par fonctionnalité, qui commence par l'**espace de
  travail** (`['units', id, …]`) : changer d'espace ne mélange jamais les caches.
- **Annulation** : toujours passer `signal` (`AbortSignal` = `CancellationToken`).
- **Après une écriture** (`useMutation`) : invalider les clés concernées
  (`queryClient.invalidateQueries({ queryKey: orderKeys.all(unitId) })`) ; pas de mise à
  jour « optimiste » sans raison forte.
- Réglages par défaut du `QueryClient` : `staleTime` court (30 s) pour les listes, long
  (24 h) pour les lookups ; **pas de nouvelle tentative** sur un 4xx (seulement réseau/5xx,
  une fois) ; pas de rechargement au retour sur l'onglet pour les formulaires ouverts.
- Déconnexion : `queryClient.clear()`.

## L'adresse

- L'**espace de travail** (et la période choisie, s'il y en a une) sont des segments ou
  paramètres de l'adresse ; un hook `useWorkspace()` les lit et fournit les permissions.
- Les **filtres, le tri, la page, l'aperçu ouvert** d'une liste sont dans la chaîne de
  requête, via un hook de la fonctionnalité (`useOrderFilters`) qui lit, valide (valeur
  inconnue → valeur par défaut) et écrit `useSearchParams`. Changer un filtre ou le tri
  remet la page à 1.
- Noms de paramètres **courts et lisibles** dans la langue de l'adresse du projet
  (`?statut=…&page=2`), stables : ce sont des liens partagés.
- ⚠ **Exception : rien de personnel dans l'adresse.** Un critère qui nomme une personne ou
  porte un numéro (recherche par nom, par téléphone) reste dans l'état de la page et part dans
  le corps d'un `POST` de recherche (lu par `useQuery` comme une lecture) : une adresse finit
  dans l'historique, les journaux des serveurs et l'en-tête `Referer`. D'une page à l'autre,
  l'historique ne porte qu'un identifiant opaque ; le reste passe en mémoire.

## Formulaires (React Hook Form)

- `useForm<CreateOrderRequest>({ defaultValues })` ; les composants maison se branchent
  par `Controller`.
- Validation locale seulement pour le confort (obligatoire, longueur) **avec les mêmes
  messages que l'API** ; l'API reste seule juge.
- À l'envoi : bouton `busy` ; sur `VALIDATION_FAILED`, un utilitaire commun place chaque
  `validationErrors[Clé]` sur son champ (`Email` → `email`, `Address.City` →
  `address.city`) et la clé `""` en erreur générale ; focus sur le premier champ en erreur.
- Ressource modifiable : la `version` reçue repart avec l'envoi ; `CONCURRENCY_CONFLICT` →
  dialogue « modifiée entre-temps » + recharger, sans perdre la saisie affichée.
- Quitter avec des modifications non enregistrées : confirmation (`useBlocker`).
