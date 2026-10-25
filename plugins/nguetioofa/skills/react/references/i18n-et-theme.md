# Traduction, formats, thème, densité

## Traduction (i18next + react-i18next) — dès la première ligne

- Langues : **fr** (défaut, repli) et **en** ; choisie par l'utilisateur (menu du profil),
  mémorisée par appareil, sinon la langue du navigateur ramenée à fr/en.
- **Espaces de noms** : `common` (boutons, états, navigation), `errors` (messages génériques
  côté front), et **un par fonctionnalité** (`orders`, `payments`) dans
  `features/<f>/i18n/fr.json` + `en.json`, chargés avec la fonctionnalité.
- Dans un composant : `const { t } = useTranslation('orders')` ; `t('list.title')`.
- **Clés sémantiques** (`list.title`, `actions.collect`), jamais le texte en clé ; variables
  nommées (`"recorded": "Paiement de {{amount}} enregistré"`) ; pluriels par suffixes
  `_one` / `_other` (`t('count', { count })`).
- **Toute clé ajoutée en fr l'est en en dans le même geste.** Un test (ou un script de
  vérification) compare les deux fichiers de chaque espace de noms : clé absente = échec.
- **Pas de texte dans le code** : ni dans le JSX, ni dans un `aria-label`, ni dans un
  `placeholder`, ni dans un `title`.
- La langue de l'interface part dans **`Accept-Language`** à chaque requête ; les messages
  d'erreur de l'API arrivent **déjà traduits** (`detail`) : ils ne sont pas des clés du front.
- Libellés des énumérations et référentiels : **depuis l'API** (lookups), pas dans les
  fichiers de traduction.
- `<html lang>` mis à jour au changement de langue.

## Formats (`Intl`, dans `ui-core`)

Fonctions pures et testées, qui prennent la langue affichée :

```ts
formatMoney(1845000, 'fr', 'EUR')  // « 1 845 000,00 € »  (en : « €1,845,000.00 »)
formatDate(iso, 'fr', tz)          // « 14/03/2008 »       (dates-heures UTC → fuseau de l'espace)
formatPhone('+33612345678')        // « +33 6 12 34 56 78 » (groupes selon l'indicatif)
formatRelative(iso, 'fr')          // « il y a 5 min »
```

- La **devise** et le **fuseau** viennent de l'API (réglage de l'espace), jamais d'une
  constante du front ; une devise sans centimes (XAF, JPY) s'affiche sans décimales.

- Jamais de `toLocaleString()` éparpillé dans les composants, jamais de découpe de chaîne
  de date : `Intl.DateTimeFormat` avec `timeZone`.
- Une date **sans heure** (`2008-03-14`) ne se convertit pas de fuseau.

## Thème clair / sombre — dès la première ligne

- Les **rôles** de tokens sont redéfinis par `[data-theme="dark"]` ; les composants et pages
  n'utilisent que des rôles : ils sont justes dans les deux thèmes sans rien faire.
- **Script de thème dans `index.html`, avant les styles** : lit la préférence (`system` par
  défaut, `light`, `dark`), pose `data-theme` sur `<html>` (pas de flash au chargement),
  suit le changement de thème du système, repasse en **clair pour l'impression**
  (`beforeprint` / `afterprint`).
- En React : un `ThemeProvider` expose `preference` et `setPreference` ; le choix se fait dans
  le menu du profil (Système / Clair / Sombre, icônes `monitor` / `sun` / `moon`).
- Images et logos : prévoir le fond (tuile claire pour un logo dessiné pour le blanc).
- **Tout écran se vérifie dans les deux thèmes** avant d'être déclaré fini.

## Densité et responsive

- `data-density` sur `<html>` : confortable (défaut), compacte (listes longues au bureau),
  tactile (automatique sur écran tactile, cibles de 44 px).
- Largeurs de référence à vérifier : **1440**, **1024**, **768**, **360** px.
