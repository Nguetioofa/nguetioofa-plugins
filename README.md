# nguetioofa-plugins

Marketplace personnelle de plugins Claude Code. Elle contient un plugin, **`nguetioofa`** :

| Élément | Nom une fois installé | Rôle |
|---|---|---|
| skill | `nguetioofa:dotnet` | méthode pour tout projet .NET : API, MVC, Razor Pages, Blazor, tâche de fond ; gabarit de socle d'API dans `skills/dotnet/assets/socle/` |
| skill | `nguetioofa:react` | méthode pour tout front React + TypeScript |
| agent | `nguetioofa:dev-back` | développeur .NET (Opus) |
| agent | `nguetioofa:dev-front` | développeur React (Opus) |
| agent | `nguetioofa:verif` | relecteur à froid, lecture seule (Opus) |
| agent | `nguetioofa:aide` | tâches mécaniques (Haiku) |

Les agents ne savent rien d'un projet en particulier : ils lisent son `CLAUDE.md` et, s'il en
désigne, ses **règles des agents** (ports, dossiers de travail, préfixes des données de test,
bases d'essai, qui committe).

## Démarrer un projet

Des prompts prêts à coller pour chaque type de projet (API, API + front, MVC, Blazor, front seul,
worker), pour intégrer un projet existant, ouvrir une phase, reprendre un ancien système, et créer
les skills spécialisés d'un projet : [`prompts/prompts-de-demarrage.md`](prompts/prompts-de-demarrage.md).

## Installer sur une machine

Prérequis : Claude Code et git. Dans un terminal :

```bash
claude plugin marketplace add Nguetioofa/nguetioofa-plugins
claude plugin install nguetioofa@nguetioofa-plugins
```

(ou `/plugin marketplace add Nguetioofa/nguetioofa-plugins` et `/plugin install …` dans une
session `claude`). Dans une session déjà ouverte : `/reload-plugins`.

Si le dépôt devient privé, la machine doit y accéder par git **sans invite** : identifiants
enregistrés dans le gestionnaire d'identifiants de git (sous Windows, Git Credential Manager), ou
une clé SSH chargée dans `ssh-agent`. Le tester :

```bash
git ls-remote https://github.com/Nguetioofa/nguetioofa-plugins.git
```

## Mettre à jour

Après avoir poussé une nouvelle version (monter `version` dans
`plugins/nguetioofa/.claude-plugin/plugin.json`, sinon les machines gardent leur copie) :

```bash
claude plugin marketplace update nguetioofa-plugins
claude plugin update nguetioofa@nguetioofa-plugins
```

La mise à jour automatique se règle par machine : `/plugin` → Marketplaces → Enable auto-update.

## Partager

Le dépôt est public : chacun l'installe avec les deux mêmes commandes. Pour le rendre privé,
le régler sur GitHub et donner l'accès à qui doit l'installer.

## Avant de pousser

```bash
claude plugin validate ./plugins/nguetioofa --strict
claude plugin validate .
```

Essai sans installer : `claude --plugin-dir ./plugins/nguetioofa`.
