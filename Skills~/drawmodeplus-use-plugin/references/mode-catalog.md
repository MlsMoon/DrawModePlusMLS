# DrawModePlus: Mode Catalog

Authoritative list of `DrawModePlusMode` values and how they render.

Source: `Runtime/DrawModePlusRuntimeState.cs`,
`Runtime/DrawModePlusRendererFeature.cs`, `Editor/DrawModePlusModeRegistry.cs`.

## Enum

| Value | Display name | Kind |
|---|---|---|
| `None` | None | Restores built-in Shaded |
| `Depth` | Depth | Fullscreen |
| `WorldNormalForward` | WorldNormal(Forward) | Fullscreen |
| `WorldNormalDeferred` | WorldNormal(Deferred) | Fullscreen |
| `BaseColorDeferred` | BaseColor(Deferred) | Deferred GBuffer |
| `MaterialAO` | MaterialAO | Deferred GBuffer |
| `MetallicDeferred` | Metallic(Deferred) | Deferred GBuffer |
| `RoughnessDeferred` | Roughness(Deferred) | Deferred GBuffer |
| `TexelDensity` | TexelDensity | Scene + tag |
| `UV0` | UV0 | Scene override |
| `Stencil` | Stencil | Stencil write + blit |
| `Reflection` | Reflection | Scene override |

Section name in SceneView: `DrawModePlusMLS`.

## Shader / pass map

| Mode | Shader | Feature path |
|---|---|---|
| Depth | `DrawModePlus/DepthView` | `FullscreenDebugPass` (depth input) |
| WorldNormal Forward | `DrawModePlus/WorldNormal` | `FullscreenDebugPass` (depth + normal) |
| WorldNormal Deferred | `DrawModePlus/DeferredNormalBuffer` | `FullscreenDebugPass` (depth + normal) |
| MaterialAO | `DrawModePlus/DeferredDebugView` pass 0 | Capture then composite pass 2 |
| BaseColor | `DrawModePlus/DeferredDebugView` pass 1 | Same |
| Metallic | `DrawModePlus/DeferredDebugView` pass 3 | Same |
| Roughness | `DrawModePlus/DeferredDebugView` pass 4 | Same |
| TexelDensity | Host tag + `DrawModePlus/FlatGray` | `TexelDensityDebugPass` |
| UV0 | `DrawModePlus/UV0Checker` | `Uv0DebugPass` |
| Stencil | `StencilWriter` + `StencilChecker` | `StencilDebugPass` |
| Reflection | `DrawModePlus/ReflectionView` | `ReflectionDebugPass` |

## Camera filter

`DrawModePlusRendererFeature` draws SceneView cameras always.
Game cameras require `enableGameView`.
Preview cameras never draw.

`DrawModePlusRenderPipelineBridge.IsDeferred` decides whether GBuffer modes enqueue.
If the camera is not Deferred, those modes return without drawing.

## Scene overlay hints

`CustomDrawModeInitializer` draws a bottom hint for:

- TexelDensity — density legend
- Reflection — chrome IBL reminder
