# Gestes d'écriture, lots, portée

Comment s'écrit une opération qui change l'état, seule ou en lot. Ces règles complètent
`tranche-verticale.md` (la forme d'un service) et `ef-core.md` (transactions, concurrence).

---

## Un geste

- **Un geste = un service dédié** (ou un « éditeur » qui réunit les gestes d'une ressource) :
  une transaction, **les verrous en tête**, la ressource relue **suivie** dans la portée de la
  permission (`LoadForUpdateAsync`), **la version jugée sous le verrou**, le geste qui juge tout
  avant de changer quoi que ce soit, l'enregistrement, une trace au journal, la ressource relue
  rendue. Un geste qui ne change rien n'écrit rien et ne trace rien.
- **La version périmée se juge avant tout le reste** (forme, codes, cohérence, état) : la
  personne doit d'abord savoir qu'elle regarde une ressource ancienne. L'exception de
  concurrence à l'enregistrement n'est que le filet d'une course.
- **La version d'une ressource couvre ses liens** (lignes filles, arbre, liaisons) : quand seuls
  eux changent, toucher la ressource (`Touch`) pour que sa version bouge.
- **Ordre des verrous fixe et écrit** (le parent avant l'enfant ; dans un projet multi-tenant,
  l'organisation, puis l'unité ; jamais l'inverse) : deux gestes
  qui prennent les mêmes verrous dans un ordre différent finissent en interblocage. Verrou
  pessimiste par `SELECT … FOR UPDATE` sur la ligne parente (`LockAsync`), `FOR SHARE` pour
  relire une donnée dont on dépend sans la changer.
- **Un drapeau `can…` publié sur une ressource dit exactement ce que l'API acceptera**, dans la
  portée de la permission qui écrit. **Une borne que l'API juge se publie** (dates permises,
  longueur, quota restant) pour que l'écran l'applique avant d'envoyer.
- **Une action ne porte qu'une permission.** Une action qui en demande deux porte la première
  sur sa route et fait juger la seconde par son service **avant toute lecture** (403).
- **Un nombre publié sur une ressource commune compte dans la portée de la personne**, jamais
  un total qui révélerait ce qu'elle ne voit pas.

## Ce qui décide un accès

- **Une règle de sécurité ne se déduit jamais d'une coïncidence** (deux dates égales, un ordre
  d'insertion, une valeur nulle) : l'information s'écrit dans une colonne.
- **Une énumération qui décide d'un accès n'a pas de valeur à zéro**, et sa règle s'écrit en
  positif : nommer ce qui ouvre, jamais ce qui ferme (une valeur oubliée reste fermée).
- **Un état réparti sur plusieurs colonnes se lit par une seule fonction** qui les lit toutes.
- **Une règle n'a qu'un endroit** (une classe `…Rules` ou `…Access`) : le service, le
  validateur, le drapeau `can…` et le lot l'appellent ; personne ne la recopie.
- **Une ressource hors de la portée reçoit exactement la réponse d'une ressource inexistante**
  (`404 ENTITY_NOT_FOUND`), même message, même durée.

## Les lots

- **Ce qui est faux dans la demande refuse tout le lot** (400), jugé sur la demande seule,
  avant de lire une ligne — le refus ne dit rien d'une ligne hors de la portée.
- **Ce qui tient à l'état d'une ligne n'écarte qu'elle**, dit par une énumération par ligne
  (`…Outcome`, libellée par `GET /api/lookups`), jamais par un texte. Un `200` ne veut pas dire
  « tout réussi » : la réponse porte l'issue de chaque ligne.
- Les mêmes verrous dans le même ordre qu'un geste seul ; **un seul enregistrement** ; une
  trace par ligne changée (`WriteManyAsync`).
- **Un quota se juge pour tout le lot, d'un coup, avant d'écrire.**
- **Proposition et application lisent la même chose** ; l'application **recalcule sous les
  verrous** (la proposition a pu vieillir). Une ligne périmée dans un lot versionné refuse le
  lot entier (`409`), pour que l'écran relise.
- **Un lot ne lit jamais ligne par ligne** (une requête pour toutes les lignes) et **ne modifie
  jamais sa source** : rejoué, il rend « déjà fait » pour chaque ligne.

## Petits gestes qui évitent de gros défauts

- **Une propriété qui ne change plus après la création se déclare `init`**, et aucune demande de
  modification ne la porte (un test le garde par réflexion).
- Suppression logique d'une entité lue avec ses dépendants suivis : **détacher ceux-ci
  d'abord** (EF refuserait de rompre une clé étrangère obligatoire).
- Un geste refusé qui doit être tracé (tentative d'accès à une donnée sensible) se trace
  **après** la transaction annulée, hors d'elle.
- Une lecture qui écrit (« vu le… ») est un geste : elle se nomme et se documente comme tel.
