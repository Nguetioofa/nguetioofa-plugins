# Gestes, formulaires, lots

Ce qui rend une saisie sûre : on ne perd jamais ce que la personne a tapé, on n'envoie jamais
deux fois, on ne montre jamais un succès comme un échec ni l'inverse. Complète la section
« Formulaires » de `etat-et-donnees.md`.

---

## Ouvrir, envoyer

- **Une page de modification attend une lecture fraîche** avant d'afficher le formulaire (un
  hook `useFreshOnEntry`) ; le formulaire n'est jamais démonté une fois affiché.
- **Tout geste passe par une garde de double envoi** (`useSendGuard`) : `isPending` seul ne
  suffit pas (deux clics dans le même rendu partent tous les deux).
- Un formulaire ne se remet jamais (`reset`) pendant un rendu : l'état relu adopté passe par un
  effet.
- Une fenêtre de geste **ne se ferme jamais d'elle-même sur un refus** ; un refus de toute la
  demande va en tête de la fenêtre, focus compris ; un `400` sous son champ par son chemin
  (listes `…[i]` sous la liste), focus sur le premier.
- Après une fenêtre ou une confirmation, **le focus revient au déclencheur** ; si le déclencheur
  a disparu (ligne supprimée), à un repli désigné.

## Conflits et relectures

- La `version` part avec l'envoi. **`409 CONCURRENCY_CONFLICT` : la saisie est gardée**,
  « Recharger » la remplace à la demande seulement.
- **Un état relu pendant une saisie ne s'adopte jamais par-dessus elle** : une seule fonction
  juge (`rereadOutcome` : `adopt` si rien n'est saisi, `follow` si la saisie est déjà égale,
  `conflict` sinon, signalé).
- Un lot qui reçoit un `409` « changé entre-temps » ne renvoie rien : « Recharger » relit et
  rejoue les choix des seules lignes qui n'ont pas bougé ; les autres sont signalées sous la
  ligne, le texte tapé restant visible (« Reprendre », « Laisser »).
- Un refus qui ferme l'écriture pendant une saisie (période close, lecture seule) : la ressource
  relue s'adopte, la saisie reste visible en lecture avec le refus en tête, jusqu'à « Annuler
  les modifications ».
- Un geste à part qui rend la ressource d'un formulaire ouvert (changer un logo pendant une
  modification) lui donne sa nouvelle `version` sans écraser la saisie.

## Quitter

- Fermer ou quitter un formulaire modifié se confirme (une garde `useBlocker`) ; levée seulement
  quand la réussite a rendu la saisie propre, jamais pendant l'envoi ; la garde déclare chaque
  paramètre d'adresse dont la saisie dépend (changer de période = quitter).
- Une navigation qui suit un enregistrement part au rendu suivant (après la levée de la garde).
- Une saisie partagée par deux vues d'une même ressource vit dans leur route parente ; sa garde
  laisse passer de l'une à l'autre.

## Cache après un geste

- **Une mutation dont la réponse est la ressource entière remplace le cache** de cette
  ressource ; ce qu'elle ne couvre pas est relu (`invalidateQueries`).
- **Sous un même préfixe de clé, une seule forme de donnée** (les pages d'une liste sous
  `…'list'`, un compte sous sa propre clé) : un `setQueriesData` par préfixe ne voit jamais autre
  chose.
- **Une réussite de l'API n'est jamais montrée comme un échec** : la mise à jour du cache qui
  suit ne lève pas (au pire, on relit).

## Lots et assistants

- Un geste en lot : la **proposition** (rien d'écrit), puis le **compte rendu tiré de la
  réponse**, dans le même panneau. **Un `200` n'est pas « tout réussi »** : l'issue de chaque
  ligne vient de la réponse.
- Un assistant ne garde que **les écarts** de la personne par rapport à la proposition de
  l'API ; la proposition se relit sans rien faire perdre, et ce qui a changé est dit.
- Des lignes cochées : gardées en mémoire d'une page à l'autre, **jamais dans l'adresse** ; une
  ligne que l'API refuserait (`can…` faux) ne se coche pas et dit pourquoi.
- Un lot qui s'enregistre de lui-même (une grille de saisie) n'adopte une relecture que **case
  par case** ; une case changée en ligne pendant la saisie se choisit (« la vôtre / en
  ligne »), rien n'est écrasé ; il ne réessaie seul que sur une coupure, un `5xx` ou un `429` ;
  rien ne part après le démontage.

## Nouvelles tentatives

- `429` : le message de l'API, puis une nouvelle tentative après `Retry-After` **quand c'est une
  lecture**.
- **Un geste qui n'est pas rejouable sans effet** (émettre un document numéroté, payer) : jamais
  renvoyé d'office ; `429` / `503` disent « rien n'a été fait », le nouvel essai est voulu par la
  personne. Un geste en plusieurs envois est figé au clic, confirmé envoi par envoi, et ne
  repart jamais du début une fois un envoi fait.
- Une attente de `Retry-After` s'abandonne quand la page se démonte : rien ne s'enregistre
  depuis une autre page.

## Fichiers

- Un fichier rendu par l'API se lit par une fonction du paquet `api` (`unwrapFile`, nom tiré de
  `Content-Disposition`) et s'enregistre par une seule fonction de l'application ; ses en-têtes
  utiles se lisent sur la réponse.
- Un fichier à envoyer reste en mémoire ; ce que l'écran peut contrôler seul (type, taille) l'est
  avant l'envoi ; un refus du fichier entier tombe sur la zone de dépôt.
