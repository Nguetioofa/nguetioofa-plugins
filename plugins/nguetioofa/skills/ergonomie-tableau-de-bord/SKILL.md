---
name: ergonomie-tableau-de-bord
description: Ergonomie d'une application de gestion qui a un tableau de bord, fondée sur la recherche (NN/g, Stephen Few, Tableau Research, Shneiderman, Cowan, Bastien et Scapin, SAP Fiori, Carbon) — organiser le tableau de bord et la page d'accueil, placer chaque chose à l'écran, structurer le menu et la navigation, décider combien de clics une fonctionnalité peut demander, alléger la charge mentale, choisir indicateurs et graphiques, adapter l'écran au rôle, et auditer un écran existant. À utiliser dès qu'on conçoit, revoit, critique ou audite un tableau de bord, une page d'accueil d'application, un menu latéral, une barre de navigation, une arborescence, un parcours vers une fonctionnalité importante, des indicateurs (KPI), un écran par rôle, ou qu'on rédige un prompt de maquette pour un tel écran — même si l'utilisateur dit seulement « dashboard », « accueil », « menu », « combien de clics », « UX » ou « c'est mal rangé ». Le système d'UI du projet (son skill d'UI) l'emporte pour l'apparence ; ce skill décide de l'organisation.
---

# Ergonomie d'une application à tableau de bord

Ce skill dit **où mettre les choses et combien d'effort elles peuvent coûter** dans une
application de gestion (administration, back-office, SaaS) qui a un tableau de bord. Il
s'appuie sur des études et des guides reconnus ; chaque règle porte son niveau de preuve,
pour qu'on sache ce qui est démontré, ce qui fait consensus et ce qui est un choix raisonné.

- **Le système d'UI du projet l'emporte pour l'apparence** (couleurs, composants, textes). Ce
  skill décide de l'**organisation** : quoi montrer, où, dans quel ordre, à combien de clics.
- **Le `CLAUDE.md` du projet l'emporte** quand il a déjà tranché.
- Pour le dessin des graphiques eux-mêmes (couleurs de séries, axes, légendes), un skill de
  visualisation de données, s'il y en a un, prend le relais.

Niveaux de preuve utilisés partout :

| Marque | Sens |
|---|---|
| **[étude]** | mesuré sur des utilisateurs (étude publiée) |
| **[consensus]** | guide d'experts ou système de design reconnu, sans mesure chiffrée |
| **[choix]** | synthèse de ce skill à partir des deux premiers ; à ajuster au projet |

---

## 1. Les dix règles à retenir

1. **Aucun nombre de clics maximum n'est démontré.** La « règle des 3 clics » est fausse
   [étude : Porter, 44 personnes, 620 tâches, plus de 8 000 clics]. Ce qui fait échouer, c'est
   de **ne pas savoir où cliquer**, pas le nombre de clics. Chaque clic doit être évident.
2. **L'effort se règle sur la fréquence** [choix] : une tâche quotidienne à 1 geste depuis
   l'accueil ou le menu (plus un raccourci) ; une tâche hebdomadaire à 2 ou 3 gestes évidents ;
   une tâche rare au bout d'un chemin clair (fil d'Ariane, recherche). Détail :
   [`clics-et-chemins.md`](references/clics-et-chemins.md).
3. **Pas plus de 2 niveaux** de divulgation progressive ni de menus en cascade [consensus :
   NN/g]. Au-delà, on simplifie ou on regroupe.
4. **L'accueil montre 1 à 4 tâches principales**, chacune avec un point d'entrée net, et
   **l'information la plus attendue sans aucun clic** [consensus : Nielsen, NN/g].
5. **Le menu principal est à gauche, en colonne, visible, avec des libellés** [consensus et
   étude : NN/g, la moitié gauche de l'écran reçoit environ 80 % des regards]. Jamais d'icône
   seule, jamais de menu « hamburger » sur ordinateur, **jamais un menu qui se réordonne
   seul**.
6. **Le haut gauche porte l'essentiel, mais la position ne suffit pas** [étude : Tableau
   Research 2026]. On lit d'abord le titre, puis les gros chiffres et les textes courts ; un
   élément important mais terne finit lu en dernier, un élément voyant mais secondaire vole
   l'attention. Position, mise en valeur et sens doivent dire la même chose. Détail :
   [`placement-et-lecture.md`](references/placement-et-lecture.md).
7. **Un tableau de bord sert un parcours** [consensus : Few, Shneiderman] : vue d'ensemble →
   ce qui demande mon attention → détail à la demande → départ vers l'action. Un indicateur
   sans contexte (cible, tendance, comparaison) ne dit rien. Détail :
   [`organiser-le-tableau-de-bord.md`](references/organiser-le-tableau-de-bord.md).
8. **Peu d'éléments, regroupés** [étude : Cowan, la mémoire immédiate tient environ 4 blocs].
   Plus de choix ralentit la décision (Hick) ; une cible grande et proche se touche plus vite
   (Fitts). Détail : [`charge-mentale.md`](references/charge-mentale.md).
9. **L'écran s'adapte au rôle, avec un bon défaut** [étude : NN/g]. Adapter par rôle marche
   bien ; le réglage par chaque personne sert peu de monde ; sans un bon écran par défaut, la
   plupart des gens n'ont rien.
10. **On décide avec les utilisateurs, pas au goût** [consensus] : tâches principales votées,
    tri par cartes, test d'arborescence, tests avec environ 5 personnes, test des 5 secondes.
    Détail : [`methode-et-mesure.md`](references/methode-et-mesure.md).

---

## 2. Concevoir un tableau de bord ou une page d'accueil

Suivre ces étapes dans l'ordre ; chacune répond à une question.

1. **Pour qui, pour quelle décision ?** Lister les rôles qui ouvriront l'écran et, pour chacun,
   les décisions qu'il doit prendre et à quel rythme (chaque matin, chaque semaine, en
   fin de période). Un écran sans décision à servir n'est pas un tableau de bord.
2. **Quel genre d'écran ?** [consensus : NN/g, Carbon]
   - **opérationnel** : surveiller et agir vite (ce qui est en retard, à valider, anormal) ;
   - **analytique** : comprendre une tendance, creuser avec filtres et détail ;
   - **portail / accueil** : lancer ses tâches.

   Ne pas mélanger les trois sur un même écran : un accueil peut porter un bloc opérationnel
   et des raccourcis, l'analyse va sur sa propre page.
3. **Quelles tâches et quels chiffres ?** Classer chaque tâche par **fréquence × importance**
   (idéalement mesurées : journaux d'usage, vote des tâches principales). Les 1 à 4 premières
   vont sur l'accueil ; le reste dans le menu.
4. **Disposer en zones** [choix, à partir de Few, Tableau, Carbon, Fiori] — de haut en bas :

   | Zone | Contenu | Pourquoi |
   |---|---|---|
   | En-tête | titre qui dit où l'on est (espace, période, périmètre) | le titre est lu en premier [étude] |
   | Attention | ce qui demande une action, compté et cliquable (« 12 absences à justifier ») | le parcours commence par « quoi traiter » [consensus] |
   | Indicateurs | 3 à 7 chiffres clés, chacun avec cible, tendance ou comparaison | gros chiffres lus tôt [étude] ; mémoire limitée [étude] |
   | Tâches | 1 à 4 entrées directes vers les tâches principales | [consensus : Nielsen] |
   | Détail | graphiques, listes récentes, ce qui sert moins souvent | plus bas = perçu moins important [consensus : Few] |

5. **Relier chaque élément à son action.** Un chiffre ou une alerte mène, en un clic, à la
   liste déjà filtrée qui permet d'agir ; la liste garde le filtre visible et un retour clair.
6. **Vérifier** avec la grille de la section 4, puis avec des utilisateurs (section 10 des
   règles).

---

## 3. Organiser le menu et la navigation

- **Colonne à gauche** pour une application qui a plus de 5 ou 6 rubriques ; barre du haut pour
  le contexte global (recherche, changement d'espace, notifications, compte) [consensus].
- **Libellés** alignés à gauche, mot important en premier, vocabulaire du métier de
  l'utilisateur (pas celui de la base de données) [consensus : NN/g].
- **Regrouper** sous des intitulés de section plutôt qu'ajouter des niveaux ; « 7 ± 2
  rubriques » n'est pas une limite démontrée, un menu à gauche peut en porter davantage s'il
  est groupé [consensus : NN/g].
- **Arborescence de profondeur moyenne** : trop profonde, on se perd ; trop large et plate,
  on rate le bon lien. Une profondeur moyenne avec des libellés clairs l'emporte [étude :
  Larson et Czerwinski, CHI 98].
- **Repères** : rubrique active marquée, fil d'Ariane dès 3 niveaux, titre de page identique
  au libellé du menu [consensus].
- **Stabilité** : même menu sur tous les écrans, même ordre, rien qui apparaît ou disparaît
  selon l'usage ; on peut **ajouter** des favoris ou des récents en tête sans **déplacer** les
  entrées [consensus : NN/g, mémoire spatiale].
- **Masquer ce qu'on ne peut pas faire** dans le menu (permission absente) plutôt que le griser
  sans explication ; dans une page, un geste refusé dit pourquoi [choix].
- **Recherche et palette de commandes** comme second chemin pour les habitués, jamais comme
  le seul [consensus : NN/g, accélérateurs].
- **Raccourcis vers les pages complémentaires** dans chaque page [choix] : un lien à côté de
  la donnée qu'il prolonge (il ouvre la page cible déjà filtrée), et une rangée « Voir aussi »
  de 2 à 4 liens sous le titre, aux libellés du menu, filtrés par les permissions. Détail :
  [`clics-et-chemins.md`](references/clics-et-chemins.md), § 7.

---

## 4. Auditer un écran existant

Pour une revue, parcourir cette grille et noter chaque point **conforme / à revoir / grave**,
avec l'écran, l'élément en cause et la correction proposée.

**Orientation**
- [ ] Le titre dit où l'on est (espace, période, périmètre) et se lit en premier.
- [ ] Un nouvel utilisateur saisit le message principal en 5 secondes.

**Priorités**
- [ ] Ce qui demande une action est en haut, compté, cliquable.
- [ ] Les indicateurs sont peu nombreux (3 à 7) et ont chacun un contexte.
- [ ] Rien de voyant n'est secondaire ; rien d'important n'est terne.
- [ ] La couleur vive est réservée à ce qui demande l'attention ; jamais seule porteuse du sens.

**Chemins**
- [ ] Les tâches quotidiennes du rôle sont à 1 geste de l'accueil ou du menu.
- [ ] Chaque clic du chemin vers une tâche importante est évident (libellé, pas d'icône seule).
- [ ] Pas plus de 2 niveaux de menus ou de divulgation.
- [ ] Un chiffre mène à la liste filtrée qui permet d'agir, avec retour clair.
- [ ] Chaque page mène à ses pages complémentaires : liens en contexte et rangée « Voir aussi »
      (2 à 4 liens, libellés du menu, selon les permissions).

**Navigation**
- [ ] Menu à gauche, visible, libellé, stable, rubrique active marquée.
- [ ] Le vocabulaire est celui du métier.

**Charge et confort**
- [ ] Au plus environ 4 blocs à garder en tête pour comprendre l'écran.
- [ ] Les cibles sont assez grandes (au moins 24 × 24 px, WCAG 2.2) et les actions
      fréquentes proches de leur contexte.
- [ ] Les gestes courants répondent en moins d'une seconde, avec un retour visible ; au-delà
      de 10 secondes, une progression.

**Rôles**
- [ ] Chaque rôle a un défaut utile sans rien régler.
- [ ] Ce que le rôle ne peut pas faire n'encombre pas son écran.

Présenter le compte rendu ainsi :

```markdown
# Audit d'ergonomie — <écran>

## En bref
<2 ou 3 phrases : ce qui marche, le problème principal>

## Points graves
- **<élément>** — <règle enfreinte, niveau de preuve> → <correction>

## À revoir
- …

## Conformes
- …
```

---

## 5. Idées reçues à ne pas répéter

| Idée reçue | Ce qui est établi |
|---|---|
| « Tout doit être à 3 clics. » | Faux [étude : Porter ; NN/g]. Compte l'évidence de chaque clic et la fréquence de la tâche. |
| « 7 ± 2 éléments au plus. » | Miller parlait de mémoire immédiate, pas de menus ; la limite mesurée est plutôt d'environ 4 blocs [étude : Cowan], et un menu visible se reconnaît, il ne se retient pas. |
| « 80 % de l'attention va en haut à gauche d'un tableau de bord. » | Chiffre sans étude derrière lui. Le chiffre mesuré (NN/g) concerne la **moitié gauche** de l'écran, sur des pages web. |
| « Les gens lisent en F. » | Vrai pour du texte ; sur un tableau de bord, la lecture suit plutôt haut-gauche → bas-droite, déviée par ce qui est voyant [étude : Tableau 2026]. |
| « Moins il y a d'éléments, mieux c'est. » | Désencombrer n'est pas un but : un expert peut vouloir un écran dense, et cacher ajoute des clics. On retire ce qui ne sert pas une décision [consensus]. |
| « Chaque utilisateur personnalisera son tableau de bord. » | La plupart ne règlent rien ; c'est le défaut par rôle qui compte [étude : NN/g]. |

---

## Fiches

| Fiche | À lire quand |
|---|---|
| [`clics-et-chemins.md`](references/clics-et-chemins.md) | on décide du chemin vers une fonctionnalité, du nombre de gestes, des raccourcis |
| [`placement-et-lecture.md`](references/placement-et-lecture.md) | on dispose un écran : ce qui est vu d'abord, où mettre menu, titre, chiffres, filtres |
| [`organiser-le-tableau-de-bord.md`](references/organiser-le-tableau-de-bord.md) | on choisit indicateurs, cartes, graphiques, couleurs, écran par rôle, états vides |
| [`charge-mentale.md`](references/charge-mentale.md) | un écran paraît chargé ou lent à comprendre ; critères de Bastien et Scapin |
| [`methode-et-mesure.md`](references/methode-et-mesure.md) | on prépare une conception ou une vérification avec des utilisateurs, ou un prompt de maquette |
| [`sources.md`](references/sources.md) | on doit citer une source ou vérifier ce qu'elle prouve vraiment |
