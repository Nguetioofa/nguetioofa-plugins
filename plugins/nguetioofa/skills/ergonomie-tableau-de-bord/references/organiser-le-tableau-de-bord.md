# Organiser le tableau de bord

## 1. Ce qu'est un tableau de bord

- **Stephen Few** : un affichage **sur un seul écran** des informations les plus importantes
  pour faire son travail, conçu pour la surveillance rapide. Il doit tout montrer clairement,
  sans distraction, d'une manière qu'on examine et comprend vite [consensus].
- **NN/g** : une vue d'une page, d'un coup d'œil, pour agir vite ; ce n'est pas un outil
  d'exploration, et ce n'est pas un **portail** (une porte vers beaucoup de tâches) [consensus].
- Dans une application de gestion, la page d'accueil mêle souvent les deux : un bloc de
  surveillance (« ce qui demande mon attention ») et des entrées vers les tâches. C'est
  acceptable si chaque bloc garde son rôle [choix].

## 2. Le parcours à servir

[consensus : Few, 2007 ; Shneiderman, 1996]

1. **Vue d'ensemble** : que se passe-t-il ?
2. **Ce qui demande mon attention** : repéré d'un coup d'œil, sans chercher.
3. **Regarder de plus près** ce qui en demande, assez pour décider s'il faut agir.
4. **Détail à la demande** : le tableau de bord sert de point de départ direct vers
   l'information complète, et si possible vers l'action (relancer, valider, écrire).

Shneiderman le résume ainsi : vue d'ensemble d'abord, zoom et filtre, puis détails à la
demande.

## 3. Genres de tableaux de bord

| Genre | Sert à | Règles |
|---|---|---|
| **Opérationnel / présentation** | surveiller, décider vite | peu d'indicateurs, hiérarchie nette, plus important = plus grand et plus contrasté, en haut ; une même couleur pour une même série partout ; blancs pour grouper (Carbon, NN/g) |
| **Analytique / exploration** | comprendre, creuser | recherche, tri, filtres, regroupement et descente dans le détail ; graphiques liés (filtrer l'un met les autres à jour) ; légendes toujours au même endroit ; annotations qui ne cachent pas les données (Carbon) |

## 4. Les indicateurs

- **Peu** : 3 à 7 chiffres clés sur la rangée du haut [choix, à partir de Carbon « limiter les
  indicateurs », Zoho « 2 à 4 » en tête, Few « 7 » dans son exemple].
- **Toujours un contexte** [consensus : Few] : un nombre seul ne répond pas à « par rapport à
  quoi ? ». Ajouter au moins l'un de :
  - une cible ou un seuil (barre de cible, « 82 % / objectif 90 % ») ;
  - une tendance (courbe miniature sur les dernières périodes) ;
  - une comparaison (période précédente, moyenne).
- **Précision utile seulement** : pas de décimales ni de détails qui ne changent pas la
  décision [consensus : Few].
- **Chaque indicateur est cliquable** vers la liste ou la page qui l'explique, filtrée sur le
  même périmètre [choix].
- **Chiffre simple et gros** plutôt qu'un chiffre décoré d'icônes : retirer le superflu fait
  plus ressortir que d'ajouter de l'emphase (NN/g, 2020).

## 5. Les cartes

[consensus : SAP Fiori, page de synthèse]

- Une page de cartes se justifie à partir d'environ **3 cartes**, pas toutes du même type.
- **Un sujet ou une tâche par carte.**
- **Aucune carte ne répète une autre** : chacune apporte une valeur propre.
- Préférer les cartes standard du système de design ; une carte sur mesure seulement si
  aucune ne convient.
- Disposition fixe (tailles égales, peu d'interaction) ou redimensionnable (au plus 5 colonnes
  de cartes) ; l'utilisateur peut masquer des cartes.
- Ne pas utiliser une page de cartes pour un seul objet (une fiche suffit) ni pour lancer de
  simples applications (un menu suffit).

## 6. Les graphiques

[consensus : NN/g, 2017 ; Few]

| Pour montrer | Utiliser |
|---|---|
| une évolution | courbe |
| une comparaison de valeurs | barres, triées |
| une relation | nuage de points |
| une valeur contre sa cible | graphique à puce (barre + trait de cible + zones) |
| une tendance compacte dans un tableau | courbe miniature (« sparkline ») |

- **Position et longueur** se lisent avec précision ; **aire et angle** mal.
- À éviter : camemberts et anneaux (sauf parts très inégales), jauges et cadrans, cartes
  arborescentes, toute 3D, effets décoratifs.
- Pas d'icône ni d'illustration décorative : elles alourdissent la recherche visuelle.

## 7. La couleur

- **Gris et neutres par défaut, couleur vive pour ce qui demande l'attention** : dans
  l'exemple de Few, seul un point rouge signale l'indicateur en danger, et rien d'autre ne
  détourne le regard [consensus].
- Jamais la couleur seule pour porter un sens : environ 4,5 % de la population a un trouble de
  la vision des couleurs (environ 8 % des hommes, 0,5 % des femmes) ; doubler par une forme,
  une icône ou un texte [étude citée par NN/g].
- Rouge et vert disent « mauvais » et « bon » : ne les employer que quand c'est le sens voulu
  [consensus : Smashing].
- Peu de couleurs en tout ; la même couleur pour la même série sur tout l'écran [consensus :
  Carbon].

## 8. Adapter au rôle

[étude : NN/g, personnalisation et réglage]

- **Adapter par rôle** (la personne est directrice, caissière, enseignante) marche bien,
  parce que le rôle est connu d'avance et stable.
- **Deviner d'après l'historique** marche moins bien.
- **Laisser chacun régler** (masquer, réordonner des cartes) : utile à une minorité ; la
  plupart ne règlent jamais rien.
- Donc : un **bon défaut par rôle** d'abord, le réglage individuel ensuite, en option.
- Les données de rôle doivent rester à jour (sinon l'écran ment) ; l'utilisateur doit pouvoir
  comprendre pourquoi il voit ce qu'il voit.

## 9. Densité

[consensus : Smashing ; NN/g, applications complexes]

- Désencombrer n'est pas un but en soi : un utilisateur expert peut vouloir un écran dense,
  et cacher l'information ajoute des clics.
- Ce qu'on retire : ce qui ne sert **aucune décision** du rôle, ce qui répète, ce qui décore.
- Pour alléger sans perdre : hiérarchie nette, détail au survol ou dans un panneau latéral,
  blancs pour grouper, deux ou trois couleurs.
- Le détail secondaire se lit **sans quitter l'écran** (info-bulle, panneau) quand c'est
  possible (NN/g, 2020).

## 10. États particuliers

[choix]

- **Premier usage / vide** : dire pourquoi c'est vide et proposer le geste qui remplit
  (« Aucune classe : créer la première »), jamais un graphique à zéro.
- **Rien à traiter** : le bloc « attention » le dit en une ligne (« Rien à valider
  aujourd'hui ») au lieu de disparaître, pour qu'on sache que ce n'est pas une panne.
- **Chargement** : squelettes à la place des blocs, sans décaler la mise en page ; chaque bloc
  se charge et échoue seul.
- **Erreur d'un bloc** : le bloc dit l'erreur et propose de réessayer ; le reste de l'écran
  reste utilisable.
- **Données périmées** : afficher l'heure de la dernière mise à jour quand elle compte.
