---
name: controle-etape
description: Contrôle du coordinateur d'un projet après le rapport d'un agent (nguetioofa:dev-back, nguetioofa:dev-front, nguetioofa:aide, nguetioofa:verif) - build et tests, migrations à appliquer, documentation de l'API, documentation du projet, dossiers de travail, relecture à froid, puis compte rendu à l'utilisateur. À utiliser à chaque retour d'agent, avant d'annoncer une étape faite.
---

# Contrôle d'une étape

À suivre par le coordinateur à chaque rapport d'agent. Rien n'est annoncé « fait » avant d'avoir été
vérifié à l'exécution. Les commandes, chemins, bases et vérifications propres au projet sont dans
son `.claude/regles-des-agents.md` : les appliquer.

## 1. Lire le rapport

- **Questions ouvertes** : chacune va à l'utilisateur par `AskUserQuestion` (options, recommandée
  en premier), sauf si la réponse est déjà écrite dans le plan ou la conception.
- Un refus d'outil, une partie non faite ou non vérifiée, un incident (un fichier interdit lu, un
  secret affiché) : le noter pour le compte rendu, et le dire à l'utilisateur.

## 2. Build et tests (toujours)

- Backend : build et tests **compilés hors du dépôt** (dans le dossier de travail du
  coordinateur), au vert et sans avertissement ; nombre de tests comparé au rapport, tests
  d'intégration HTTP compris.
- Front : typecheck, lint, tests, build hors du dépôt (jamais dans le dépôt).
- La CI du projet est verte sur le commit, quand il y en a un.

## 3. Migrations (backend)

- Relire chaque migration du rapport, dans l'ordre donné.
- **Non cassante** (ajouts, données de départ) : l'appliquer à la base de travail si les règles du
  projet le permettent au coordinateur, puis le dire. **Cassante** (retrait ou changement d'une
  colonne que l'application en marche lit) : avec l'utilisateur.
- Après : les vérifications que le projet désigne (script de vérification du schéma, comparaison du
  modèle EF au catalogue).
- Mot de passe de la base par le mécanisme prévu par le projet, jamais affiché ; jamais de
  suppression de données hors des données de test.

## 4. À l'exécution, selon l'étape

- **Sécurité, permissions, isolation, données, argent** : le scénario complet, y compris le refus
  attendu.
- Sinon : une vérification ciblée de ce que l'étape change.
- Écran : dans le navigateur, fr et en, clair et sombre, largeur de téléphone.

## 5. Documentation

- Documentation de l'API publiée pour le front, s'il y en a un : version, ce qui a changé, ce que le
  front doit faire, là où le projet la tient.
- Plan : l'étape est à jour (preuves, décisions ouvertes, **mesure** jetons et durée face à
  l'estimation).
- `git diff` des `CLAUDE.md` et des références du skill métier du projet : seulement la ligne d'état
  et des règles courtes, **aucun récit** ; une règle de domaine dans sa référence. Corriger sinon.
- Le registre des restes du projet (feuille de route) : ajouter ce que l'étape laisse, effacer ce
  qu'elle a repris.

## 6. Ménage

- Dossiers de travail des agents supprimés ; supprimer ce qui traîne (ce qui ne peut pas l'être :
  le signaler à l'utilisateur).
- Ports d'essai libérés, bases d'essai supprimées, données de test nettoyées.

## 7. Relecture à froid

Étape sensible : lancer `nguetioofa:verif` (skill `consigne-agent`, § 4) avant de déclarer l'étape
faite.

## 8. Compte rendu à l'utilisateur

Court : ce qui est fait, ce qui est vérifié (commande et résultat), migrations appliquées, ce qui
n'est pas fait ou pas vérifié, **ce qu'il doit committer**, les questions. Mettre à jour la mémoire
d'avancement en quelques lignes (état, où lire, prochaine étape).
