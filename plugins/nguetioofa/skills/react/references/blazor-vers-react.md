# De Blazor à React

Pour expliquer une notion React à quelqu'un qui connaît Blazor. Les deux se ressemblent
beaucoup : des **composants** qui reçoivent des **paramètres**, gardent un **état**, et se
redessinent quand il change. La différence principale : React est écrit en
**TypeScript** (du JavaScript typé), et le « balisage » s'écrit **dans** le code (JSX), au lieu
d'un fichier Razor où le code est dans `@code`.

---

## 1. Un composant, côte à côte

Blazor — `CustomerCell.razor` :

```razor
<div class="cell-main">
    <a href="@Href">@Customer.LastName @Customer.FirstName</a>
    @if (Customer.IsOverdue)
    {
        <span class="badge badge--danger">@L["Customers.Overdue"]</span>
    }
</div>

@code {
    [Parameter] public CustomerDto Customer { get; set; } = default!;
    [Parameter] public EventCallback<CustomerDto> OnOpen { get; set; }
    [Inject] IStringLocalizer<SharedResource> L { get; set; } = default!;
    string Href => $"/clients/{Customer.PublicId}";
}
```

React — `CustomerCell.tsx` :

```tsx
import { useTranslation } from 'react-i18next';
import type { CustomerListItem } from '@projet/api';

// Les « [Parameter] » deviennent les propriétés (props), décrites par un type.
interface CustomerCellProps {
  customer: CustomerListItem;
  onOpen: (customer: CustomerListItem) => void;   // EventCallback<CustomerDto>
}

// Un composant = une fonction qui reçoit ses propriétés et RENVOIE le balisage (JSX).
export function CustomerCell({ customer, onOpen }: CustomerCellProps) {
  const { t } = useTranslation('customers');       // @inject IStringLocalizer
  const href = `/clients/${customer.publicId}`;    // propriété calculée : une simple variable

  return (
    <div className="cell-main">                    {/* class → className */}
      <a href={href} onClick={() => onOpen(customer)}>
        {customer.lastName} {customer.firstName}   {/* @valeur → {valeur} */}
      </a>
      {customer.isOverdue && (                      // @if → condition && balisage
        <span className="badge badge--danger">{t('overdue')}</span>
      )}
    </div>
  );
}
```

Utilisation : `<CustomerCell customer={c} onOpen={openPreview} />`, comme
`<CustomerCell Customer="c" OnOpen="OpenPreview" />` en Blazor.

## 2. Le tableau de correspondance

| Blazor | React | À savoir |
|---|---|---|
| composant `.razor` | fonction dans un fichier `.tsx` | un composant par fichier, nommé comme lui |
| `[Parameter]` | propriétés (**props**), typées par une `interface` | en lecture seule : un composant ne modifie jamais ses props |
| `EventCallback<T>` | propriété fonction : `onChange`, `onOpen` | on l'appelle : `onOpen(customer)` |
| `RenderFragment ChildContent` | `children` | `<Card>…contenu…</Card>` |
| `@if` / `@foreach` | `{condition && …}` / `{liste.map((x) => <Ligne key={x.publicId} … />)}` | **`key` obligatoire** dans une liste (comme `@key`) : l'identifiant stable, jamais l'indice |
| champ privé + `StateHasChanged()` | `const [ouvert, setOuvert] = useState(false)` | appeler `setOuvert(true)` **redessine** le composant (pas de `StateHasChanged`) |
| `OnInitializedAsync` qui charge des données | **`useQuery`** (TanStack Query), dans le `api.ts` de la fonctionnalité | chargement, erreur, cache, annulation gérés ; **pas** de `useEffect` pour charger |
| propriété calculée | variable dans la fonction, ou `useMemo` si le calcul est coûteux | le composant est ré-exécuté à chaque rendu : c'est normal |
| `IDisposable.Dispose` | fonction de nettoyage renvoyée par `useEffect` | rare : abonnements, minuteries |
| `@inject MonService` | un **hook** (`useCustomers()`, `useWorkspace()`) ou un contexte | pas de conteneur d'injection |
| `CascadingValue` / `[CascadingParameter]` | **contexte** : `createContext` + `useContext` | thème, langue, espace de travail, utilisateur |
| `@bind-Value` | valeur **contrôlée** : `value={x}` + `onChange={(e) => setX(e.target.value)}` | dans un formulaire : React Hook Form (`register`) |
| `EditForm` + `DataAnnotationsValidator` | **React Hook Form** ; erreurs de l'API (`validationErrors`) affichées sous les champs | l'API reste seule juge |
| `@page "/clients/{id}"` | une route dans `routes.tsx` + `useParams()` | |
| `[SupplyParameterFromQuery]` | `useSearchParams()` | filtres, tri, page : dans l'adresse |
| `NavigationManager.NavigateTo(...)` | `const navigate = useNavigate(); navigate(...)` | |
| `IStringLocalizer["Clé"]` + `.resx` | `t('clé')` + fichiers `fr.json` / `en.json` | toute clé en fr **et** en |
| isolation CSS `.razor.css` | **CSS Modules** : `Page.module.css` + `className={styles.grille}` | mise en page seulement ; tokens uniquement |
| `Program.cs` (services, pipeline) | `main.tsx` + fournisseurs dans `app/` | |
| `async Task` / `await` | `async` / `await` (une `Promise` = une `Task`) | |
| `CancellationToken` | `AbortSignal` | TanStack Query le fournit à chaque requête |
| DTO `record` / classes | types TypeScript **générés** depuis l'OpenAPI (`@projet/api`) | on ne les réécrit jamais à la main |
| `?.` `??` `is null` | `?.` `??` `=== undefined` / `=== null` | TypeScript strict = nullabilité en erreur |
| `IJSRuntime` | inutile : on est **déjà** en JavaScript | |

## 3. Les projets et les commandes

| .NET | Ici |
|---|---|
| solution `.slnx` avec plusieurs projets | **monorepo pnpm** : `pnpm-workspace.yaml` liste `apps/*` et `packages/*` |
| `.csproj` + `<PackageReference>` | `package.json` + `"dependencies"` |
| `Directory.Packages.props` (versions centralisées) | **catalogue** dans `pnpm-workspace.yaml` ; les `package.json` écrivent `"catalog:"` |
| NuGet | registre npm |
| `dotnet restore` | `pnpm install` |
| `dotnet watch run` | `pnpm dev` (rechargement à chaud : la page se met à jour à l'enregistrement) |
| `dotnet build -c Release` | `pnpm build` (produit des fichiers statiques dans `dist/`) |
| `dotnet test` | `pnpm test` |
| référence de projet | dépendance `"@projet/ui-web": "workspace:*"` |

## 4. Quatre règles qui surprennent en venant de Blazor

1. **Les hooks s'appellent toujours, dans le même ordre**, en haut de la fonction : jamais
   dans un `if`, une boucle ou après un `return`. (React les retrouve par leur rang.)
2. **L'état ne se modifie pas, il se remplace** : `setListe([...liste, nouvel])`, pas
   `liste.push(nouvel)` — sinon React ne voit pas le changement et ne redessine pas.
3. **Un composant se ré-exécute souvent** (à chaque changement de ses props ou de son état) :
   la fonction doit rester **pure** — elle calcule et renvoie du balisage, sans effet de
   bord (pas d'appel réseau, pas d'écriture).
4. **En développement, React exécute certains effets deux fois** exprès (mode strict), pour
   révéler les oublis de nettoyage. Ce n'est pas un bug ; en production, une seule fois.

## 5. Petit lexique

| Mot | Sens |
|---|---|
| **JSX** | le balisage écrit dans le TypeScript (`<div>…</div>`) |
| **props** | les paramètres d'un composant |
| **state** (état) | les données propres à un composant, qui provoquent un nouveau rendu quand elles changent |
| **hook** | fonction qui commence par `use…` et donne accès à l'état, au contexte, aux données (`useState`, `useQuery`, `useTranslation`) |
| **rendu** | l'exécution d'un composant qui produit son balisage |
| **contexte** | valeur transmise à tous les composants en dessous, sans passer de props (`CascadingValue`) |
| **SPA** | application d'une seule page : le navigateur charge une fois, puis change d'écran sans recharger |
| **HMR** | rechargement à chaud pendant le développement |
| **bundle** | les fichiers JavaScript et CSS produits par `pnpm build` |
| **workspace** | un des projets du monorepo (`apps/web`, `packages/ui-web`) |
