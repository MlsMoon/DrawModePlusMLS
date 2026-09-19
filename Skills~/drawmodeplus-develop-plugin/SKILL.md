---
name: drawmodeplus-develop-plugin
description: Maintain and extend the DrawModePlusMLS plugin. Covers Renderer Feature auto-inject, pass routing, SceneView mode registry, Reflection override redraw, ResourceFinder, and verification. Use when editing Runtime/ or Editor/, adding a draw mode, changing inject contracts, or debugging a blank SceneView mode.
---

# DrawModePlus: Develop the Plugin

Maintenance guide for edits under this plugin root.

Integration contracts live in the sibling skill `drawmodeplus-use-plugin`.
Current-contract overrides: [`../../AGENTS.md`](../../AGENTS.md).

## Layered architecture

```text
SceneView camera mode / control panel
  |
  | 1. EDITOR REGISTRY — Editor/DrawModePlusModeRegistry.cs
  |    SceneView.AddCameraMode(name, "DrawModePlusMLS")
  |    ApplyMode writes DrawModePlusRuntimeState and the SceneView cameraMode
  v
  2. MODE HANDLERS — Editor/DrawModes/*.cs : CustomDrawModeBase
     OnInitialize registers. OnSceneViewSelected sets the enum.
  |
  | 3. AUTO-INJECT — Editor/CustomDrawModeInitializer.cs
  |    [InitializeOnLoad] injects DrawModePlusRendererFeature into every
  |    URP ScriptableRendererData. Feature is a sub-asset of that renderer.
  v
  4. RUNTIME FEATURE — Runtime/DrawModePlusRendererFeature.cs
     Editor-only AddRenderPasses. Routes CurrentMode to a pass.
     Layouts: references/architecture.md
```

| File | Role |
|---|---|
| `Runtime/DrawModePlusRuntimeState.cs` | Current mode, depth meter, Force Mirror, roughness |
| `Runtime/DrawModePlusRendererFeature.cs` | Pass enqueue, shader materials, Game View gate |
| `Runtime/DrawModePlusRenderPipelineBridge.cs` | URP asset / renderer / Deferred reflection |
| `Runtime/Passes/FullscreenDebugPass.cs` | Depth / world-normal blit |
| `Runtime/Passes/SceneObjectDebugPass.cs` | Shared `DrawRenderers` override |
| `Runtime/Passes/ReflectionDebugPass.cs` | Clear + override IBL redraw |
| `Runtime/Passes/TexelDensityDebugPass.cs` | FlatGray then host LightMode |
| `Editor/CustomDrawModeInitializer.cs` | Load hook, inject, overlay hints |
| `Editor/DrawModePlusModeRegistry.cs` | Dropdown names and SceneView apply |
| `Editor/DrawModePlusControlWindow.cs` | `Tools/DrawModePlus/Draw Mode Control Panel` |
| `Editor/ResourceFinder.cs` | Plugin root from `DrawModePlus.asmdef` |

## Design invariants

- Player builds must not require these passes. Keep `#if UNITY_EDITOR` on enqueue.
- Reflection is an override redraw. Do not drive URP Lighting Debug again.
- Auto-inject is idempotent: skip if a `DrawModePlusRendererFeature` already exists.
- `ResourceFinder` uses `AssetDatabase` + `DrawModePlus.asmdef`. Do not parse
  `CallerFilePath` or assume `Assets\Textures`.
- Scene override tags stay `SRPDefaultUnlit`, `UniversalForward`,
  `UniversalForwardOnly`, `UniversalGBuffer`.
- English only in comments, XML docs, skills, root README, and git messages.
  Translations go in `Docs/`.

## Pitfalls

1. **Deferred modes on a Forward renderer.** `IsDeferred` is false → GBuffer
   modes enqueue nothing. That is not a shader miss.
2. **Inject into the wrong Renderer Data.** SceneView uses the pipeline default
   renderer unless the camera overrides it. Inject all renderers in the list.
3. **Shader.Find after a failed import.** Missing graphs log a warning and skip
   that mode. Do not silently fall back to another mode.
4. **Stale Reflection comments.** Older notes said Lighting Debug. The pass is
   `ReflectionDebugPass` + `DrawModePlus/ReflectionView`.
5. **TexelDensity gray.** Host shaders must add the LightMode. Do not special-case
   a host project's shader inside this repo.

## Adding a mode

Follow `references/adding-a-mode.md`. Minimum set: enum value, display name,
DrawModes class, Feature routing, shader if needed.

## Verification

Do not add TEST scripts or test asmdefs.

1. **Compile.** After host Unity compile, `DrawModePlus.dll` and
   `DrawModePlus.Runtime.dll` must be newer than the sources you changed.
2. **Smoke.** SceneView: cycle every dropdown mode and the control panel.
   Depth slider and Force Mirror must update the view immediately.
3. **Path split.** Deferred GBuffer modes on a Deferred renderer. Forward
   WorldNormal on a Forward renderer. Reflection / UV0 / Stencil on both.
4. **Cleanup.** Mode `None` or Feature disabled → Shaded, no leftover blit.

## Git commits

Use English Conventional Commits (`type(scope): description`). See `AGENTS.md`.
Do not rewrite commits that already exist on `origin`. Local unpushed subjects
may be restated in English.

## References

- `references/architecture.md`
- `references/adding-a-mode.md`
- Sibling use skill for the public mode catalog
