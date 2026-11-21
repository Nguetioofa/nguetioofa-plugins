# Clics et chemins vers une fonctionnalité

## 1. Ce que dit la recherche sur le nombre de clics

**La règle des 3 clics n'a aucune base.**

- **Origine** : le livre de Jeffrey Zeldman (2001) la cite comme une idée déjà répandue, sans
  donnée. Aucune étude publiée ne l'a jamais confirmée (NN/g, 2019).
- **Joshua Porter (UIE, 2003)** [étude] : 44 personnes, 620 tâches, plus de 8 000 clics.
  - Aucun lien entre le nombre de clics et la réussite : les tâches réussies et ratées ont la
    même répartition de longueurs.
  - On n'abandonne pas plus après 3 clics qu'après 12 ; presque personne n'a abandonné au
    troisième.
  - Certains ont ouvert jusqu'à 25 pages ; 80 % des parcours n'étaient finis qu'autour de 15
    clics.
  - La part d'insatisfaits reste entre 46 et 61 % quel que soit le nombre de clics.
  - Conclusion de Porter : les gens se plaignent des clics quand ils **ne trouvent pas** ;
    quand ils trouvent, ils ne comptent pas.
- **Limite de cette étude** : des sites web et des tâches de recherche d'information. Elle ne
  dit rien des gestes répétés cent fois par jour par un employé ; c'est là que le nombre de
  gestes compte vraiment (voir § 3).

## 2. Ce qui remplace le compte des clics : l'effort d'interaction

L'effort d'interaction (NN/g) est la somme de ce qu'un chemin coûte : lire, chercher, hésiter,
se tromper, attendre, revenir. Compter les clics n'en mesure qu'une petite partie : ouvrir un
accordéon ne coûte pas un chargement de page, et **3 clics lents coûtent plus que 5 clics
rapides** (NN/g).

Ce qui réduit vraiment l'effort [consensus : NN/g] :

| Levier | Comment |
|---|---|
| **Odeur de l'information** | chaque libellé annonce clairement ce qu'il y a derrière ; pas de terme vague (« Divers », « Outils »), pas de nom de marque interne |
| **Repérage** | rubrique active marquée, fil d'Ariane, sous-navigation locale : on sait où l'on est et comment revenir |
| **Pages carrefour** | à un point clé d'un parcours en plusieurs étapes, une page qui présente la suite et permet de revenir |
| **Tâches principales en surface** | les tâches les plus importantes sont proposées dès l'accueil et les pages clés |
| **Vitesse** | un chargement court ; un geste qui répond en moins d'une seconde |
| **Méga-menu plutôt que cascade** | un panneau à plat plutôt que des sous-menus imbriqués, sources d'erreurs de souris |

## 3. Le budget d'effort par fréquence [choix]

Faute de règle démontrée, on règle l'effort sur la **fréquence × importance** de la tâche
pour le rôle concerné :

| Tâche (pour ce rôle) | Budget | Moyens |
|---|---|---|
| **Plusieurs fois par jour** (encaisser, faire l'appel, valider) | 1 geste depuis l'accueil ou le menu ; sur place, 0 détour | entrée directe sur l'accueil ou en tête du menu, raccourci clavier, action en ligne dans la liste, valeurs par défaut sensées |
| **Chaque jour ou chaque semaine** | 2 ou 3 gestes, **tous évidents** | rubrique du menu + page de la tâche ; recherche |
| **Mensuelle, en fin de période** | chemin clair, quelle que soit sa longueur | menu groupé, fil d'Ariane, page carrefour, assistant si les étapes sont indépendantes |
| **Rare ou réglage** | profondeur acceptée | « Réglages », recherche, aide |
| **Urgente ou risquée** (alerte, échéance) | **0 clic pour la voir**, 1 pour agir | bloc « attention » de l'accueil, notification, compteur dans le menu |

Ce tableau se vérifie avec des utilisateurs (taux de réussite, temps, erreurs : voir
[`methode-et-mesure.md`](methode-et-mesure.md)), pas en comptant.

## 4. Divulgation progressive

[consensus : Nielsen, 2006]

- Montrer d'abord les quelques options les plus utiles ; le reste sur demande. Le simple fait
  qu'une option soit sur le premier écran dit qu'elle est importante.
- Deux choses à réussir :
  1. **le partage** entre premier plan et second plan, fondé sur l'usage réel (journaux, analyse
     des tâches, tests) ;
  2. **le passage** vers le second plan : un contrôle visible, au libellé qui dit ce qu'on va
     trouver.
- **Pas plus de 2 niveaux** : au-delà, on se perd. S'il en faut 3, simplifier d'abord, sinon
  regrouper les options avancées en blocs.
- **Divulgation par étapes** (assistant) : seulement si les étapes sont indépendantes. Si l'on
  doit faire des allers-retours entre elles, un seul écran vaut mieux. Exemple cité :
  réserver un hôtel en 2 écrans a battu 1 écran et 5 écrans.
- Dans une application complexe, une option n'apparaît que quand elle a un sens (une
  sous-option sous la case qui l'active) (NN/g, 2020).

## 5. Menus en cascade et profondeur

- **Pas plus de 2 niveaux** de menus déroulants imbriqués : suivre un couloir de sous-menu à
  la souris est lent et source d'erreurs (loi de l'acheminement, NN/g). Un panneau à plat
  (méga-menu) échappe à ce problème ; Fitts s'applique toujours : cibles larges.
- **Profondeur moyenne** : un site très profond fait perdre du temps ; le plus large et plat
  n'est pas le meilleur non plus. Une structure moyenne, avec de bons libellés, a donné les
  meilleurs résultats [étude : Larson et Czerwinski, CHI 98].
- **Raccourcis vers le bas** d'une structure profonde : liste alphabétique, éléments les plus
  consultés, récents (NN/g).

## 6. Accélérateurs : raccourcis, palette de commandes, recherche

[consensus : NN/g, 2024]

- Un accélérateur est un **second chemin** pour les habitués ; le débutant l'ignore sans rien
  perdre. Jamais le seul moyen de faire une chose.
- Le réserver aux **actions fréquentes et centrales**.
- Le faire découvrir **peu à peu** : afficher le raccourci à droite dans le menu ou dans
  l'info-bulle, une astuce après qu'on a fait le geste à la main, une à la fois.
- Ne pas détourner les raccourcis communs (copier, coller, tout sélectionner, imprimer).
- Garder les mêmes raccourcis partout ; donner un retour visible et un « annuler ».
- Palette de commandes (`Ctrl+K`) : flèches pour parcourir, Entrée pour choisir, Échap pour
  fermer ; elle montre le raccourci de chaque commande. Éviter les raccourcis d'une seule
  lettre, déclenchés par erreur par la saisie vocale.

## 7. Raccourcis vers les pages complémentaires

[choix, à partir de NN/g : repérage, sous-navigation locale, pages carrefour, détail sans
quitter l'écran]

Une page mène directement à celles qui la prolongent : pour une suite évidente, on ne
repasse pas par le menu. Le menu reste la carte complète ; les raccourcis sont les chemins
courts. Deux formes :

1. **Le lien en contexte** (à préférer) : à côté de la donnée qu'il prolonge (compteur,
   alerte, état, ligne, compte rendu d'un geste), il ouvre la page cible **déjà filtrée** sur
   le même périmètre et dit ce qu'il ouvre (« 12 absences non justifiées · Justifier en
   lot »). Un geste qui aboutit propose sa suite logique (« Imprimer le reçu »). Une page de
   réglages mène à la page qu'elle règle, et inversement.
2. **La rangée « Voir aussi »** : sous le titre de la page, 2 à 4 liens vers les pages
   voisines, dans l'ordre des passages les plus fréquents. Le haut de page est lu en
   premier [étude : Tableau 2026] ; un bloc en bas de page serait vu en dernier, un menu
   déroulant « Aller à » cacherait les liens.

Règles :
- libellé **identique à l'entrée du menu** (même odeur de l'information) ; jamais « Cliquez
  ici », « Plus » ni une icône seule ;
- un **lien**, pas un bouton : il navigue, il ne fait rien ; les actions restent à leur place ;
- filtré par les **permissions** comme le menu : pas de lien grisé, rangée absente s'il ne
  reste rien ;
- garde le périmètre (année, classe, période), jamais une donnée personnelle dans l'adresse ;
- ne répète ni un onglet, ni le fil d'Ariane, ni l'action principale ; au plus 4, sinon la
  page est mal découpée ou il faut une page carrefour ;
- même place sur toutes les pages, dans l'ordre de tabulation après le titre et les actions ;
  sur téléphone, la rangée passe à la ligne au lieu de se cacher.

Le choix des liens d'une page part des **passages réels** (journaux d'usage : d'où vient-on,
où va-t-on ensuite) ou, à défaut, du parcours de travail du rôle.

## 8. Temps de réponse

[consensus : Nielsen, d'après Miller 1968 et Card, Moran et Newell]

| Délai | Ressenti | À faire |
|---|---|---|
| ≤ 0,1 s | instantané | rien |
| ≤ 1 s | la pensée continue, le délai se remarque | retour discret |
| ≤ 10 s | l'attention tient à peine | indicateur d'attente |
| > 10 s | l'attention est perdue | progression, possibilité de faire autre chose, avis à la fin |

Le « seuil de Doherty » (400 ms, IBM 1982) est souvent cité comme cible pour les gestes
courants ; son attribution est discutée, le garder comme ordre de grandeur.
