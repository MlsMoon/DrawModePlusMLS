# DrawModePlusMLS — agent entry

This repository is a Unity editor plugin: extra SceneView debug draw modes for URP.
Read a skill before writing code. Do not invent APIs from memory or from older README copies.

## Route

| Task | Read first |
| --- | --- |
| Install, switch modes, texel-density host pass, Game View, troubleshooting | [`Skills~/drawmodeplus-use-plugin/SKILL.md`](Skills~/drawmodeplus-use-plugin/SKILL.md) |
| Edit `Runtime/` or `Editor/`, add a draw mode, change inject/pass contracts | [`Skills~/drawmodeplus-develop-plugin/SKILL.md`](Skills~/drawmodeplus-develop-plugin/SKILL.md) |
| Human overview | [`README.md`](README.md) |

Skills live in `Skills~/` so Unity ignores them when this folder sits under `Assets/`.
Copy a skill folder into the host project's `.agents/skills/` or `.cursor/skills/` if the agent only auto-loads those paths.

## Hard rules

- URP only. Do not add Built-in or HDRP paths unless the user explicitly asks.
- Editor debug views only. Do not make the Renderer Feature required in Player builds.
- Do not add Unity Test scripts, test asmdefs, or Test Runner scaffolding.
- Verify signatures in this repo before calling them. Cross-session memory is untrusted.
- Agent-facing text in this repository is English: `AGENTS.md`, `Skills~/`, XML comments,
  README, and git commit subjects/bodies.
- Translations belong in `Docs/` (`README.zh-Hans.md`, future locales). Do not put
  non-English prose back into the root README or skills.
- Documentation screenshots live in `Docs/images/`. Sample scenes live in `Example/`.

## Git commits

Format: `type(scope): description`

- `type`: `feat` / `fix` / `docs` / `refactor` / `perf` / `test` / `chore` / `style` / `build` / `ci` / `revert`
- `scope` (optional): `drawmode`, `runtime`, `editor`, `example`, `docs`
- Description: English, imperative, no trailing period, subject ≤ 72 characters
- Examples:
  - `fix(editor): resolve plugin textures from the editor asmdef`
  - `docs(drawmode): add AGENTS.md and English skills`

## Current contract (do not use older docs)

These facts supersede any README or skill text that still mentions the old behavior.

- Reflection is a Renderer Feature **override redraw**, not URP Lighting Debug.
- `DrawModePlusRuntimeState.ForceMirror` defaults to `true` (perceptual roughness 0).
  Off uses `ReflectionRoughness`, not the original material roughness.
- TexelDensity draws `ShaderTagId("DrawModePlusTexelDensity")` on top of a FlatGray pass.
  Host shaders must add that LightMode. Gray means "not integrated", not "low density".
- Auto-inject writes `DrawModePlusRendererFeature` onto every URP `ScriptableRendererData`
  in the active pipeline. It is a sub-asset of that Renderer Data.
- `ResourceFinder` locates the plugin root from `DrawModePlus.asmdef` (Assets copy or UPM).
  Texture loads use `Arts/Textures/...`.
- SceneView section name is `DrawModePlusMLS`. Control panel menu is
  `Tools/DrawModePlus/Draw Mode Control Panel`.
- Fullscreen modes: Depth, WorldNormalForward, WorldNormalDeferred.
  Deferred GBuffer modes: BaseColor, MaterialAO, Metallic, Roughness.
  Scene redraw modes: TexelDensity, UV0, Stencil, Reflection.
- Editor UI, comments, skills, and root docs are English. Chinese lives in `Docs/`.

## Verify after edits

1. Confirm `Library/ScriptAssemblies/DrawModePlus.dll` and `DrawModePlus.Runtime.dll`
   are newer than the sources you changed.
2. Open a SceneView. Cycle every mode in the dropdown and the control panel.
3. Deferred modes only appear correctly on a Deferred renderer. Forward modes still work
   on Deferred for Depth / UV0 / Stencil / Reflection / TexelDensity.
4. Disable or remove the injected Feature: SceneView must return to Shaded with no leftover blit.
