# Méthode : décider avec les utilisateurs et mesurer

## 1. Avant de dessiner

[consensus : Smashing, 2021 ; NN/g]

Copier le tableau de bord d'un concurrent montre son apparence, jamais ses raisons. On part
de l'utilisateur :

1. **Entretiens** avec quelques personnes de chaque rôle :
   - quelles décisions prenez-vous, à quel rythme ?
   - quelle information vous manque, où la cherchez-vous aujourd'hui ?
   - qu'est-ce qui vous fait perdre du temps ?
   - qu'avez-vous besoin de voir dès que vous arrivez ?
2. **Documents réels** du métier (registres, cahiers, tableaux Excel, papiers imprimés) :
   leur ordre et leur vocabulaire sont souvent le bon modèle (critère de **compatibilité**).
3. **Tâches principales (Top Tasks, Gerry McGovern)** :
   - dresser une liste de 50 à 80 tâches, dans les mots des utilisateurs ;
   - faire choisir à chacun ses 5 plus importantes, la liste présentée d'un bloc ;
   - le résultat forme un « long cou » : une poignée de tâches recueille l'essentiel des
     voix, une longue traîne en reçoit très peu. Le haut du classement se stabilise vite,
     même avec peu de votants.
   - le cou va sur l'accueil et en tête du menu ; la traîne reste accessible, sans prendre
     la place.

   Pour une petite équipe, une version légère suffit : la même liste, votée par 10 à 30
   personnes [choix].
4. **Tri par cartes** pour bâtir le menu : écrire les fonctionnalités sur des cartes, faire
   grouper et nommer les groupes par les utilisateurs.
5. **Test d'arborescence** pour valider le menu sans maquette : « où cliqueriez-vous pour
   … ? » ; mesurer la réussite, le premier clic et les retours en arrière.

## 2. Vérifier une maquette ou un écran

- **Test des 5 secondes** : montrer l'écran 5 secondes, le cacher, demander ce qu'il dit et
  ce qui demande l'attention. Si le message principal n'est pas retenu, la hiérarchie est à
  revoir.
- **Tests d'utilisabilité** sur les tâches principales, en itérations, des croquis au
  prototype : environ 5 personnes par tour trouvent l'essentiel des problèmes ; mieux vaut
  plusieurs petits tours qu'un grand [consensus : Nielsen].
- **Penser à voix haute** : on observe où la personne hésite ; une hésitation sur un clic est
  un défaut d'odeur de l'information.
- **Premier clic** : un premier clic juste est fortement lié à la réussite de la tâche ;
  c'est l'indicateur le plus simple à suivre (à vérifier dans la source primaire avant de
  citer un chiffre).

## 3. Mesures

| Mesure | Ce qu'elle dit | Remplace |
|---|---|---|
| **Taux de réussite** d'une tâche | trouve-t-on ? | le compte des clics |
| **Temps** pour la réussir | l'effort réel, lecture et attente comprises | — |
| **Erreurs et retours en arrière** | les clics non évidents | — |
| **Premier clic juste** | la qualité des libellés | — |
| **Satisfaction** après la tâche (une question, ou le questionnaire SUS à 10 questions) | le ressenti | — |
| **Usage réel** (journaux) : fréquence des pages, chemins suivis, recherches sans résultat | ce qui doit monter ou descendre | les suppositions |

## 4. Rédiger un prompt de maquette

Quand un outil de maquette doit dessiner un tableau de bord ou un accueil, le prompt donne
[choix] :

1. **le rôle** et ses **décisions** (pas seulement « tableau de bord de la direction ») ;
2. **le genre** d'écran (opérationnel, analytique, accueil) ;
3. **l'ordre des zones** : en-tête avec périmètre, attention, indicateurs (avec leur
   contexte : cible, tendance, comparaison), tâches principales, détail ;
4. **la liste exacte** des éléments « attention » et des indicateurs, avec ce sur quoi chacun
   ouvre ;
5. **les raccourcis** de chaque écran : sa rangée « Voir aussi » et ses liens en contexte,
   avec ce qui ouvre chacun (permission, rôle) ;
6. **les états** : vide, rien à traiter, chargement, erreur d'un bloc, lecture seule ;
7. **les tailles d'écran** visées et ce qui change sur téléphone ;
8. **ce qu'il ne faut pas faire** : jauges, camemberts, 3D, icônes décoratives, couleur seule
   porteuse de sens, menu sans libellés ;
9. **le système d'UI du projet** à suivre pour l'apparence.
