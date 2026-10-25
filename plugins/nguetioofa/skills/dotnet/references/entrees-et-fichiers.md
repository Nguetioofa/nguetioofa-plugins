# Ce que l'API reçoit : textes, paramètres, fichiers

Tout ce qui vient du client est faux jusqu'à preuve du contraire. Ces règles s'appliquent dans
le socle (filtres et liaison globaux), pas fonctionnalité par fonctionnalité.

---

## Textes reçus

Une seule classe décide (`Common/TextRules`) ; un filtre global (`RequestTextFilter`, **avant**
les validateurs) remplace chaque texte de la demande par sa forme nettoyée. Validateurs,
actions et services ne voient que le texte nettoyé.

- **Retiré en silence** : les blancs des deux bords (un texte collé d'un tableur finit par une
  tabulation) et les caractères de format invisibles (catégorie Unicode `Cf`), où qu'ils soient.
- **Refusé** (400 sous le champ) : ce qui ne se devine pas — caractère de contrôle (PostgreSQL
  refuse le caractère nul), saut de ligne ou tabulation dans un texte d'une ligne, moitié de
  caractère (surrogate isolé).
- **La valeur nettoyée est la valeur** : comparée, cherchée, enregistrée, rendue.
- **Tout texte est d'une ligne, sauf marque** sur sa propriété : `[MultilineText]` (lignes
  gardées, fins de ligne en `\n`, blancs de fin de ligne retirés), `[PasswordText]` (**jamais
  nettoyé**, pas même ses bords). Un champ qu'un écran saisit sur plusieurs lignes porte la
  marque, sinon son premier saut de ligne est refusé.
- Le parcours de la demande (réflexion) couvre textes, listes et demandes imbriquées ; des
  tests refusent une forme qu'il ne sait pas lire (dictionnaire), un texte non modifiable, un
  mot de passe sans sa marque, un `[FromHeader]`.
- **Ce qui n'arrive pas par une action** (cellule d'un fichier, en-tête `User-Agent`) passe par
  `TextRules.TryClean` avant d'être gardé.
- **Un code** (identifiant court : `ADMIN`, `LOT-2`) : ASCII imprimable sans espace, jugé
  strictement sur le texte nettoyé.
- **Couper un texte à une longueur : `Truncate`**, jamais `texte[..n]` (une coupure au milieu
  d'un émoji laisse une moitié de caractère, que la base refuse).
- Deux noms qui se lisent pareil sont le même nom : comparer après nettoyage, casse ignorée,
  espaces retirés, accents comptés (une classe dédiée, jamais `StringComparer.Ordinal`).

## Paramètres d'adresse et liaison

- **Tout paramètre à une valeur lu dans l'adresse** (route, requête) passe par un liant à valeur
  unique, en tête des fournisseurs MVC : répété (`?page=1&page=2`), 400 sous sa clé, au lieu de
  garder la première valeur sans rien dire.
- **Une énumération de requête ou de route** passe par un liant strict (le texte exact du
  document OpenAPI, comme le JSON).
- **Pagination** : page bornée (`MaxPageNumber`), taille bornée, et le décalage calculé par
  une fonction (`ItemsToSkip(page, taille)`), jamais `(page - 1) * taille` (dépassement
  d'entier). Aucune propriété publique de plus sur la requête : elle sortirait dans le document OpenAPI.
- **Erreurs de validation plafonnées** (par exemple 200 champs, 10 messages par champ) : une
  demande démesurée ne produit pas une réponse démesurée.
- **Une liste a sa borne, et ses éléments ne se jugent un à un que si elle la tient**
  (`RuleForEach(…).When(…)`).
- `null` explicite dans une propriété non nullable : le message « obligatoire » sous la clé du
  champ, pas l'erreur brute du sérialiseur.
- **Les refus d'un filtre voyagent dans un `ModelState` à part** : celui de la requête ignore la
  casse, et un refus du champ `Code` sortirait sous le paramètre de route `code`.

## Données personnelles et adresses

- **Toute lecture dont les critères portent le nom d'une personne ou un numéro de téléphone est
  un `POST`** (marqué comme lecture pour le mode lecture seule) : une adresse finit dans les
  journaux des serveurs, l'historique et l'en-tête `Referer`.
- Une recherche qui révèle le titulaire d'un numéro ou d'un email est **limitée en débit**, par
  adresse IP **et** par compte.
- Un texte libre sur une personne va dans le corps d'un `POST`, jamais au journal ni dans une
  adresse.
- **Un `record` qui porte une donnée sensible remplace son `ToString()`** (le record l'imprime en
  entier dans un journal ou un message d'exception) ; s'il est hérité, il se protège par
  `PrintMembers`.
- ⚠ **Journal : `Microsoft.AspNetCore` reste à `Warning` au moins**, dans toute
  configuration : en dessous, l'hébergement écrit le chemin complet de chaque requête, donc un
  jeton porté par une adresse (invitation, vérification). Une route à jeton ne s'écrit au
  journal que par son modèle (`/invitations/{token}`).

## Corps de requête et fichiers

- **Corps borné** (2 Mo par défaut, plus sur les seules routes de dépôt par
  `[RequestSizeLimit]`) ; au-delà, `413` **dans l'enveloppe**.
- **Toute action qui reçoit un fichier à ne pas garder déclare
  `RequestFormLimits.MemoryBufferThreshold`** (sinon ASP.NET Core l'écrit sur le disque
  temporaire) ; un test parcourt l'API.
- **Un fichier déposé n'est jamais cru** : type jugé **au contenu** (signature des premiers
  octets), jamais à l'extension ni au `Content-Type` ; taille bornée avant lecture.
- **Une image reçue n'est jamais rangée telle quelle** : bornée avant décodage (dimensions,
  octets), décodée puis **réencodée** (ce qui retire métadonnées et charges cachées), un nombre
  borné de décodages simultanés. Une image d'essai des tests est générée, jamais la photo d'une
  personne.
- **Un fichier bureautique** (Excel, Word) se lit en flux par une bibliothèque reconnue (Open
  XML SDK), sans rien écrire sur le disque ; **toute analyse récursive d'un contenu client se
  borne en profondeur avant d'être lancée** (un document imbriqué à l'extrême fait déborder la
  pile et tombe l'API).
- **Un fichier produit** pour le client : `Content-Disposition` (exposé par CORS),
  `Cache-Control: no-store` s'il est personnel, erreurs dans l'enveloppe même pour une route qui
  rend un fichier. Un nom de fichier ne porte jamais le nom d'une personne.
- **Un fichier gardé** se range par une abstraction (`IFileStorage`) sous une **clé générée**,
  jamais sous le nom reçu, jamais sous `wwwroot`, jamais servi tel quel ; écrit **avant** la
  transaction et effacé si le geste échoue. Un contenu sensible (signature, pièce d'identité)
  se chiffre au repos (voir `production.md`).
