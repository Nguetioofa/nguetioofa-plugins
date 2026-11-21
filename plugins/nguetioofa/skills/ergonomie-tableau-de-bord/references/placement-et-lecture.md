# Placement et ordre de lecture

## 1. Comment on lit un tableau de bord

**Tableau Research, publié le 6 novembre 2026 (IEEE VIS 2026)** [étude] : 13 auteurs de
tableaux de bord interrogés, puis 16 utilisateurs, 64 parcours de lecture sur 8 tableaux de
bord réels.

- **L'ordre est structuré mais pas fixe.** En moyenne, la lecture va du haut gauche au bas
  droit (diagramme de Gutenberg) ; la mise en page sert d'échafaudage, pas de séquence
  imposée.
- **Le titre est le point de départ**, presque toujours, même quand il n'est pas en haut à
  gauche.
- **Textes courts et gros chiffres (« BAN ») sont lus tôt** : ils résument. Les longs
  paragraphes sont différés ou sautés, même bien placés.
- **Ce qui est voyant attire au premier passage** (grand, coloré, contrasté), même hors du
  chemin prévu, mais attirer n'est pas faire comprendre.
- **Un élément important mais terne finit en queue** : des chiffres clés placés à gauche, sans
  mise en valeur, ont été lus en dernier.
- **Filtres, onglets et sélecteurs viennent plus tard**, pour affiner ; on survole pourtant
  les graphiques et on essaie les filtres en cours de lecture.
- **Les parcours varient beaucoup** selon la personne ; même les auteurs ne prédisent qu'en
  partie le parcours de leurs lecteurs. Une même personne garde sa stratégie d'un écran à
  l'autre.

Ce qu'en tirent les chercheurs :
1. faire concorder **mise en page, mise en valeur et sens** pour que le parcours ne prête pas
   à confusion ;
2. donner des **points d'entrée** soignés : titre, gros chiffres, en-têtes ;
3. des **textes courts**, lisibles, posés à côté de ce qu'ils expliquent ;
4. des **interactions visibles** dès le début ;
5. permettre plusieurs chemins sans perdre le contexte.

**Étude Tableau antérieure (oculométrie)** [étude] : on regarde davantage le haut de l'écran ;
un indicateur clé placé à droite ou en bas d'une mise en page en colonnes est moins vu.

## 2. Le motif en F

- Le motif de lecture en F (NN/g, oculométrie) a été observé sur des **pages de texte**.
- Sur un tableau de bord, fait d'images plus que de mots, il se transpose mal ; on retient
  plutôt « haut gauche vers bas droit, dévié par ce qui est voyant ».
- Carbon (IBM) recommande quand même : le plus important en haut, puis un ordre en F, le
  moins important à la fin [consensus].

## 3. Où placer chaque élément

| Élément | Place | Source |
|---|---|---|
| Menu principal | colonne à gauche, visible, libellée | NN/g [consensus ; la moitié gauche reçoit environ 80 % des regards, étude] |
| Contexte global (recherche, espace, notifications, compte) | barre du haut | [consensus] |
| Titre de l'écran | en haut, avec le périmètre (espace, période, filtre actif) | Tableau [étude] |
| Ce qui demande une action | sous le titre, avant les indicateurs | Few, Shneiderman [consensus] |
| Indicateurs clés | haut de l'écran, gros, contrastés, en rangée | Tableau [étude], Carbon [consensus] |
| Filtres | au-dessus de ce qu'ils filtrent, visibles, état actif lisible | Tableau [étude] |
| Graphiques et listes de détail | plus bas | Few : ce qui demande de défiler est perçu moins important [consensus] |
| Entrées peu utilisées du menu | en bas du menu | NN/g [consensus] |
| Action principale d'une page | en haut, près du titre ; une seule action principale par page | [consensus] |

## 4. Le menu à gauche en détail

[NN/g, 2021]

- Convient aux architectures larges ou qui grandissent : on peut ajouter des rubriques sans
  serrer, raccourcir ou couper les libellés, ce que la barre horizontale oblige à faire.
- Prend de la place : on peut le réduire en rail d'icônes **à condition** que les libellés
  restent accessibles (survol, bouton d'ouverture) et que l'état ouvert soit le défaut sur
  grand écran [choix].
- Libellés alignés à gauche, mot clé en premier ; jamais centrés.
- Visuellement distinct du contenu (fond, bordure).
- Ne pas doubler le même menu en haut et à gauche.
- En langue qui s'écrit de droite à gauche, tout s'inverse.
- Repères chiffrés des systèmes de design [consensus] : rail de 3 à 7 destinations, barre du
  bas sur téléphone de 3 à 5, au-delà un tiroir ou un rail déplié.

## 5. Mémoire de l'emplacement

[consensus : NN/g, mémoire spatiale]

- Les habitués trouvent une entrée **par sa place**, presque sans lire. Déplacer une entrée
  casse ce réflexe.
- Un menu qui se réordonne selon l'usage est donc à éviter. Pour mettre en avant les entrées
  fréquentes, les **dupliquer** en tête (favoris, récents) en les laissant aussi à leur
  place.
- Une entrée qu'on n'a pas le droit d'utiliser peut disparaître (son absence est stable pour
  ce rôle) ; une entrée qui apparaît et disparaît selon l'état du moment trouble.

## 6. Téléphone et tablette

[choix, à partir de NN/g et Material]

- Même organisation qu'à l'ordinateur autant que possible : le menu à gauche se replie en
  tiroir, l'ordre des rubriques ne change pas.
- Sur téléphone, les 3 à 5 tâches quotidiennes du rôle en barre du bas ; le reste dans le
  tiroir.
- Le tableau de bord passe en une colonne dans l'ordre de lecture voulu : titre, attention,
  indicateurs, tâches, détail.
