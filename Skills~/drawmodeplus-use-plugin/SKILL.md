---
name: drawmodeplus-use-plugin
description: Integrate DrawModePlusMLS so a URP project gets extra SceneView debug draw modes. Covers copy/UPM install, auto-injected Renderer Feature, mode catalog, texel-density host LightMode, Reflection Force Mirror, Game View, and troubleshooting. Use when adding DrawModePlusMLS, writing a DrawModePlusTexelDensity pass, diagnosing gray texel-density objects, or switching SceneView debug views.
---

# DrawModePlus: Use the Plugin

Integration guide for the consuming project. Paths are relative to the plugin root
(`DrawModePlusMLS/`). Before coding, also read [`../../AGENTS.md`](../../AGENTS.md) —
that file is the current-contract override if any older copy of this skill disagrees.

## What the plugin does

- Registers extra SceneView camera modes under the **DrawModePlusMLS** section.
- Auto-injects `DrawModePlusRendererFeature` into every URP Renderer Data on editor load.
- Draws editor-only debug views (fullscreen blit, GBuffer capture, or scene override).
- Does not modify host gameplay materials except the optional texel-density LightMode.

## Requirements

- Unity 2022.3+
- URP 14
- Forward and/or Deferred. Built-in and HDRP are unsupported.

## 1. Install

Copy the folder under `Assets/`, or add the git URL in Package Manager:

```text
https://github.com/MlsMoon/DrawModePlusMLS.git
```

Package id: `com.mlsmoon.drawmodeplus`. After compile, open a SceneView.

Correct:

- Active pipeline is a `UniversalRenderPipelineAsset`.
- Console once logs `DrawModePlusMLS: Injected DrawModePlusRendererFeature into ...`.
- Draw Mode dropdown shows the **DrawModePlusMLS** section.

Common mistakes:

- Expecting Built-in / HDRP support.
- Looking for a `Resources/` folder. `Shader.Find` sees imported plugin shaders.
- Assuming TexelDensity works without a host `DrawModePlusTexelDensity` pass.

## 2. Switch modes

| UI | Path |
|---|---|
| SceneView dropdown | Top-left Draw Mode → section `DrawModePlusMLS` |
| Control panel | `Tools/DrawModePlus/Draw Mode Control Panel` |
| Overlay | SceneView overlay `DrawModePlusMLS Controller` (Depth + Stencil) |
| API | `DrawModePlusModeRegistry.ApplyMode(DrawModePlusMode.Depth)` |

Depth range: `DrawModePlusRuntimeState.SetDepthMeter(float)` (1–500 in the panel).
Reflection: `SetForceMirror(bool)`, `SetReflectionRoughness(float)` (0–1, Force Mirror off only).

Game cameras stay off unless **Enable Game View** is checked on the Feature.

## 3. Mode catalog (summary)

Full table: `references/mode-catalog.md`.

| Kind | Modes | How they draw |
|---|---|---|
| Fullscreen blit | Depth, WorldNormal Forward/Deferred | `FullscreenDebugPass` |
| Deferred GBuffer | BaseColor, MaterialAO, Metallic, Roughness | Capture + composite; skipped if the camera is not Deferred |
| Scene override | UV0, Reflection | `DrawRenderers` with override material |
| Scene + tag | TexelDensity | FlatGray, then `DrawModePlusTexelDensity` |
| Stencil | Stencil | Override write, then blit visualize |

Objects without `SRPDefaultUnlit` / `UniversalForward` / `UniversalForwardOnly` /
`UniversalGBuffer` are skipped by override redraws (UV0, Reflection, Stencil write).

## 4. TexelDensity host pass

Complete sample: `references/texel-density.md`.
Fictional host wiring: `references/host-integration-xx.md`.

- Add a pass tagged `LightMode = "DrawModePlusTexelDensity"`.
- Output a grayscale value normalized to 512 texels/m (or your project reference).
- Missing tag → flat gray. Gray is "not integrated", not "low density".

## 5. Reflection

- Override chrome/IBL. Not URP Lighting Debug. Not host rain/snow debug.
- Force Mirror on: roughness 0. Off: one global roughness.
- Does not include screen-space reflections from other features.

## 6. Troubleshooting

| Symptom | Fix |
|---|---|
| No DrawModePlusMLS section | Wait for compile. Confirm `[InitializeOnLoad]` ran. Reopen SceneView. |
| Modes do nothing | Feature missing on the active Renderer Data. Check auto-inject log. |
| Deferred modes black / empty | Camera / renderer is Forward. Use a Deferred renderer. |
| All TexelDensity objects gray | Host shaders lack the LightMode. Integrate the pass. |
| UV0 missing checker | `Arts/Textures/Common/ColorUV.png` failed to load. Check `ResourceFinder`. |
| Reflection looks like Lighting Debug | Stale docs. Current path is `ReflectionDebugPass` + `ReflectionView`. |
| Game View unchanged | Enable **Enable Game View** on the Feature. Preview cameras are skipped. |
| Shader not found warning | Plugin shaders were deleted or excluded from the import. |

Disable the Feature or set mode `None` to restore Shaded. No leftover blit should remain.

## 7. Checklist

- [ ] URP asset is active
- [ ] Feature injected on the renderer used by SceneView
- [ ] Dropdown and control panel switch the same `DrawModePlusRuntimeState.CurrentMode`
- [ ] Deferred modes tested on a Deferred renderer
- [ ] Host texel-density pass added only on shaders that should participate
- [ ] Did not add Unity Test scripts

## References

- `references/mode-catalog.md`
- `references/texel-density.md`
- `references/host-integration-xx.md`
- Sibling develop skill for inject/pass contracts
