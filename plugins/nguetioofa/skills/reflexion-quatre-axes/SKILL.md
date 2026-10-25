---
name: reflexion-quatre-axes
description: Grille de réflexion à quatre axes (vitesse, altitude, profondeur, portée) pour le coordinateur d'un projet. À utiliser automatiquement à l'ouverture d'une phase (exploration, cadrage, rédaction du plan, conception de l'étape 0), et quand l'utilisateur l'invoque explicitement. Ne pas l'utiliser pour exécuter une étape déjà décidée, une tâche mécanique, une petite correction, un texte fr/en, ni dans un sous-agent.
---

# Réflexion à quatre axes

Une grille pour penser avant de répondre, lors d'une **ouverture de phase** ou à la demande de
l'utilisateur. Elle ne remplace pas la méthode d'ouverture de phase du projet (explorer, cadrer,
écrire le plan, concevoir l'étape 0) : elle la nourrit.

## Les quatre axes

- **Vitesse** : ce qui permet d'avancer vite sans perdre en qualité (ce qu'on réutilise, ce qu'on
  découpe, ce qu'on délègue).
- **Altitude** : le recul et la vision d'ensemble, le contexte et les objectifs réels (métier,
  clients visés, phases déjà faites, contrat avec un front).
- **Profondeur** : le détail technique, les cas limites, ce qui peut échouer (données, permissions,
  isolation, migrations, concurrence, argent, bilingue).
- **Portée** : l'impact au-delà de la demande immédiate (autres phases, autres écrans, le contrat,
  les données existantes, la mise en service).

## Pour chaque axe, trois questions

Se placer du point de vue d'un très bon professionnel du domaine concerné, puis répondre :

1. **Matière première** : de quelle matière la plus brute partirait-il (code, base, ancien
   système, documents réels du métier, contraintes) et pourquoi ?
2. **Méthodes et outils** : quelles méthodes précises, avec quels outils concrets ? Les noms et
   versions sont **vérifiés** (dans le dépôt, la base ou par une recherche), jamais donnés de
   mémoire.
3. **Critères de qualité** : qu'est-ce qui prouverait que le résultat est bon (test, requête,
   comparaison, vérification à l'exécution, « Fait quand » mesurable) ?

## Ensuite

1. Réécrire la demande pour qu'elle exige explicitement ces trois éléments.
2. **Si la version réécrite ajoute du périmètre ou fait apparaître une décision**, ne pas
   l'exécuter telle quelle : poser chaque décision à l'utilisateur par `AskUserQuestion`
   (options expliquées, recommandée en premier). On ne code que ce qui est demandé.
3. Exécuter la tâche selon la version réécrite et validée.

## Réponse finale

Ne pas détailler la grille dans la réponse, sauf si l'utilisateur le demande. Ce qu'elle a fait
ressortir apparaît dans le résultat lui-même : options du QCM, risques du plan, « Fait quand »,
décisions ouvertes.
