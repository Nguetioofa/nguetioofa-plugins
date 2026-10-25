---
name: consigne-agent
description: Modèle de consigne du coordinateur d'un projet. À utiliser avant chaque lancement d'un agent (nguetioofa:dev-back, nguetioofa:dev-front, nguetioofa:verif, nguetioofa:aide) ou d'un agent d'exploration - vérifier que l'étape peut partir, choisir l'agent et son effort, puis écrire la consigne selon le modèle. Pas pour un sous-agent qui lance lui-même un agent.
---

# Consigne d'agent

À suivre par le coordinateur (la conversation principale) avant chaque appel à l'outil Agent. Ce
qui est propre au projet (ports, dossiers de travail, bases d'essai, qui committe, ordre des
commits, réglages d'effort de l'utilisateur) est dans son `.claude/regles-des-agents.md` et sa
mémoire : les relire, ils l'emportent sur ce skill.

## 1. L'étape peut-elle partir ?

- **Aucune décision ouverte** : métier, conception, ou décision ouverte du plan qui précède. Sinon,
  la poser d'abord à l'utilisateur par `AskUserQuestion`. Un agent ne reçoit jamais une étape dont
  une décision reste ouverte.
- **Écran important** : sa maquette (ou son prompt de design détaillé) est livrée.
- **Commits** : ce dont l'étape dépend est committé, si l'agent travaille depuis le dernier commit
  (un banc d'essai, un worktree) ou si l'application de l'utilisateur se compile depuis son arbre de
  travail. Le projet dit l'ordre dans ses règles des agents.
- **Un agent à la fois sur un même dossier partagé** (un dépôt de contrat, par exemple). Des agents
  en parallèle seulement sur des fichiers distincts, chacun dans un worktree.
- **Taille** : un morceau de taille moyenne avec son rapport. Une étape lourde se découpe plutôt que
  de monter l'effort.

## 2. Quel agent, quel effort

| Travail | Agent | Effort par défaut |
|---|---|---|
| Conception, données, sécurité, permissions, plusieurs couches, écran sur maquette, série de corrections | `nguetioofa:dev-back` / `nguetioofa:dev-front` | `medium` |
| Petite correction bien décrite, textes, données d'exemple, documentation, recopie | l'agent du dépôt, ou `nguetioofa:aide` si aucun contexte récent n'est nécessaire | `low` |
| Relecture à froid d'une étape sensible | `nguetioofa:verif` | `medium` (le projet peut exiger `high`) |
| Lire, relever, chercher dans le code | agent d'exploration, petit modèle | — |

Le barème de l'utilisateur, s'il en a donné un (dans la mémoire ou les règles des agents),
l'emporte. **Annoncer** l'agent et l'effort choisis, avec la raison en une ligne. Reprendre un agent
(`SendMessage`) seulement si son contexte est encore petit ; sinon agent neuf avec la liste précise
des corrections et des fichiers. `nguetioofa:aide` ne documente jamais le travail d'un autre agent.

## 3. Le modèle de consigne

Chaque rubrique, courte et précise. La définition de l'agent et les règles des agents du projet
disent déjà ses règles fixes : ne pas les recopier, seulement ce qui est propre à la tâche.

1. **But** : l'étape ou le morceau (chemin du plan et numéro), en deux ou trois phrases ; ce que
   « Fait quand » exige.
2. **Décisions qui s'appliquent** : leurs numéros et leur sens en une ligne chacune.
3. **À lire** : les fichiers précis (sections du plan, de la conception, documentation de l'API,
   référence du skill métier du projet utile à l'étape).
4. **Périmètre** : ce qu'il fait, ce qu'il ne fait pas, le point d'arrêt éventuel ; **ce qu'il ne
   lit jamais** (fichiers de secrets, dossiers interdits), nommé explicitement dans toute consigne
   de recherche. Une question qui n'est écrite nulle part revient dans son rapport, avec options et
   recommandation.
5. **Commentaires et tests** (toujours, même si le skill est chargé) : commentaires seulement pour
   le pourquoi non évident, au plus 1 ligne pour 5 lignes de code utiles par fichier, sans date,
   numéro de décision ni « désormais » ; tests unitaires seulement pour une règle de sécurité ou
   une logique pure qui casserait sans bruit, chacun avec sa contre-épreuve ; **tests d'intégration
   HTTP des routes de l'étape** (cas nominal et refus principaux). Ne jamais lister des sujets de
   tests comme une commande ; ne pas lui demander de compter ses commentaires.
6. **Vérifier** : ce qui doit être éprouvé à l'exécution. Une migration s'éprouve sur une copie et
   sur une base neuve avant le rapport ; un geste sans retour s'éprouve sur un banc, jamais sur la
   base commune.
7. **Documenter son propre travail** : plan (étape, preuves, décisions ouvertes), conception si
   elle change, documentation de l'API ; dans un `CLAUDE.md`, seulement la ligne d'état et une règle
   nouvelle en une ou deux lignes, jamais de récit ; une règle de domaine dans la référence du skill
   métier du projet.
8. **Rapport** : celui de sa définition, plus la **mesure** (jetons, durée) si l'étape a une
   estimation.

## 4. Relecture à froid (`nguetioofa:verif`)

Ne lui donner **ni la consigne du développeur ni ton avis** : seulement le périmètre (étape,
fichiers ou `git diff`), et s'il doit éprouver à l'exécution. Une relecture pour le backend, une
pour le front, sur chaque étape sensible (sécurité, permissions, isolation, données, argent).
