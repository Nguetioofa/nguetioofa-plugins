# Charge mentale et lois de l'interaction

## 1. La mémoire immédiate

- **Miller (1956)** : « 7 ± 2 », souvent cité. Cowan le qualifie d'estimation grossière et de
  formule rhétorique plus que de mesure.
- **Cowan (2001)** [étude] : la capacité réelle est d'**environ 4 blocs** en moyenne (moins chez
  l'enfant et la personne âgée).
- Ce qui compte, c'est le **bloc** : sa taille dépend de ce que la personne connaît déjà.
  Regrouper (un numéro en tranches, des rubriques sous un titre) permet d'en tenir plus.
- Un menu **visible** se **reconnaît** ; il ne se retient pas : la limite de mémoire ne
  plafonne donc pas le nombre de rubriques, elle plafonne ce qu'il faut **garder en tête**
  d'un écran à l'autre.

Conséquences [choix] :
- un écran se comprend à partir d'environ 4 blocs (titre, attention, indicateurs, tâches) ;
- on ne demande jamais de retenir une valeur d'un écran pour la saisir dans un autre : on la
  reporte, on la propose, on la laisse visible ;
- le filtre actif, la période et le périmètre restent affichés, pas seulement dans
  l'adresse.

## 2. Reconnaître plutôt que se souvenir

[consensus : 6e heuristique de Nielsen]

- Montrer les choix plutôt que les faire taper de mémoire.
- Ce qu'on a utilisé récemment est plus présent à l'esprit : une liste des **récents**
  (élèves, dossiers, pages) aide à reprendre un travail.
- Les entrées fréquentes peuvent être **doublées** en tête de liste, sans quitter leur place
  normale (mémoire spatiale).

## 3. Loi de Hick

Plus il y a de choix, plus la décision prend de temps [consensus].

- Réduire les choix présentés **à un moment donné**, pas les possibilités : regrouper,
  proposer un défaut, filtrer, chercher.
- Une liste longue mais triée de façon prévisible (alphabet, chronologie) se parcourt vite :
  Hick pèse surtout quand les choix ne sont pas ordonnés.

## 4. Loi de Fitts

Le temps pour atteindre une cible dépend de sa **distance** et de sa **taille** [étude :
Fitts 1954, reprise par NN/g].

- Cibles grandes, actions fréquentes **près de leur contexte** (action en ligne sur la ligne
  plutôt que tout en haut de la page).
- Les bords de l'écran sont des cibles « infinies » à la souris.
- Dans un menu linéaire, la dernière entrée est la plus lointaine : les entrées fréquentes en
  haut.
- Taille minimale d'une cible : **24 × 24 px** (WCAG 2.2, critère 2.5.8, niveau AA) ; viser
  plus pour le tactile.

## 5. Loi de l'acheminement

Suivre un couloir étroit à la souris (sous-menu en cascade) est lent et provoque des erreurs
[consensus : NN/g]. D'où : **pas plus de 2 niveaux** de menus en cascade, et des panneaux à plat
plutôt que des sous-menus imbriqués.

## 6. Attributs perçus sans effort

[consensus : NN/g, 2017]

- Certains attributs se perçoivent avant toute attention consciente : position, longueur,
  couleur, taille, forme, mouvement.
- **Position et longueur** pour les quantités (précis) ; **couleur** pour faire ressortir un
  élément parmi d'autres ou distinguer des catégories, pas pour un ordre de grandeur.
- Si tout est mis en valeur, rien ne l'est : une seule emphase forte par zone.

## 7. Critères ergonomiques de Bastien et Scapin

Synthèse française d'environ 900 recommandations (INRIA, rapport RT-0156, 1993), utilisable
en conception comme en évaluation [consensus]. Huit critères :

| Critère | Sous-critères | Question à poser à l'écran |
|---|---|---|
| **Guidage** | incitation ; groupement et distinction des éléments (par la place, par le format) ; retour immédiat ; lisibilité | sait-on quoi faire, où l'on est, et ce que le geste a produit ? |
| **Charge de travail** | brièveté (concision, actions minimales) ; densité de l'information | combien de lecture, de saisie et de gestes pour aboutir ? |
| **Contrôle explicite** | actions explicites ; contrôle par l'utilisateur | rien ne part sans geste voulu ; peut-on interrompre, annuler ? |
| **Adaptabilité** | flexibilité ; prise en compte de l'expérience | le débutant est guidé, l'habitué a ses raccourcis ? |
| **Gestion des erreurs** | protection contre les erreurs ; qualité des messages ; correction | l'erreur est-elle évitée, expliquée, réparable sans tout refaire ? |
| **Homogénéité** | — | même chose, même place, même nom, partout ? |
| **Signifiance des codes et dénominations** | — | les libellés et icônes disent-ils ce qu'ils font, dans la langue du métier ? |
| **Compatibilité** | — | l'écran suit-il la façon de travailler et les documents réels de l'utilisateur ? |

Le sous-critère **actions minimales** est la vraie formulation de « peu de clics » : limiter
les gestes nécessaires, sans sacrifier le guidage.
