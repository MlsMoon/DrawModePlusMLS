---
name: draw-mode-plus-mls-skill
description: Route DrawModePlusMLS host agents to the packaged source skills. Covers the use vs develop split and pointers to xx host cases. Use when choosing which skill to read; edit Skills~/ for install, modes, texel-density, or pass contracts.
---

# DrawModePlus: Host Router

> **This file only routes. Keep it thin.** Do not put host-project facts, paths, or
> verification logs here.
>
> Durable contracts and xx cases belong in the **source skills** (edit those):
> - Integrate / troubleshoot: [`Skills~/drawmodeplus-use-plugin/SKILL.md`](../../../Skills~/drawmodeplus-use-plugin/SKILL.md)
> - Edit plugin source: [`Skills~/drawmodeplus-develop-plugin/SKILL.md`](../../../Skills~/drawmodeplus-develop-plugin/SKILL.md)
> - Current contract overrides older docs: [`AGENTS.md`](../../../AGENTS.md)
>
> Do not copy `Skills~/` into the host `.agents/skills` as standalone skills.

## Which skill to read

| Task | Read |
|---|---|
| Install, switch modes, texel-density host pass, Game View, troubleshooting | `drawmodeplus-use-plugin` |
| Edit `Runtime/` or `Editor/`, add a draw mode, change inject/pass contracts | `drawmodeplus-develop-plugin` |

Paths are relative to the plugin root `DrawModePlusMLS/`. Use the host install path.

## xx cases live in the source skills

Fictional host **xx** cases are split under source `references/`:

| Topic | File |
|---|---|
| Host texel-density LightMode wiring | `Skills~/drawmodeplus-use-plugin/references/host-integration-xx.md` |

Do not write real Prefab / shader / Pass / renderer names back into this router
or into the public source skills.

## Checklist

- [ ] Read the matching source skill and `AGENTS.md` before writing code
- [ ] Verify APIs in this repo; do not trust an old README
- [ ] Do not add Unity Tests
- [ ] Put new reusable patterns in source-skill `references/`, not here
