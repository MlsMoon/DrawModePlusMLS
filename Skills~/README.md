# Agent skills

Two skills ship with this plugin. The folder is named `Skills~` so Unity ignores it under `Assets/`.

| Skill | When to load |
| --- | --- |
| [drawmodeplus-use-plugin](drawmodeplus-use-plugin/SKILL.md) | Install, switch modes, host texel-density pass, troubleshoot |
| [drawmodeplus-develop-plugin](drawmodeplus-develop-plugin/SKILL.md) | Edit plugin Runtime/ or Editor/ |

Moon Game Dev Tool Manager installs a thin host routing skill
(`draw-mode-plus-mls-skill` from `../.mlsmoon/`). That router only points here.
Edit these source skills, not the router. Do not copy `Skills~/` into the host
`.agents/skills` as standalone skills.
Standalone clone of this repo: read [`../AGENTS.md`](../AGENTS.md) first; many agents already load that file.
Agent-facing text (`SKILL.md`, references, git commits) is English.
Translations belong in `../Docs/`.
