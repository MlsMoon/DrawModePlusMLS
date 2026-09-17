# DrawModePlus: Architecture

## Pass routing

`DrawModePlusRendererFeature.AddRenderPasses` (editor only):

1. Skip preview cameras. Skip Game cameras unless `enableGameView`.
2. `None` → return.
3. Fullscreen (`IsFullscreenMode`) → `FullscreenDebugPass`.
4. Deferred GBuffer modes → require `DrawModePlusRenderPipelineBridge.IsDeferred`,
   then `MaterialAOCapturePass` + `MaterialAOCompositePass`.
5. TexelDensity → `TexelDensityDebugPass`.
6. UV0 → `Uv0DebugPass`.
7. Reflection → `ReflectionDebugPass`.
8. Stencil → `StencilDebugPass`.

Default pass event is `AfterRenderingPostProcessing`.
GBuffer capture is `BeforeRenderingDeferredLights`.

## Auto-inject

`CustomDrawModeInitializer` on editor load and pipeline change:

1. `EnumerateRendererData` on the active URP asset.
2. If the renderer already has `DrawModePlusRendererFeature`, skip.
3. `CreateInstance`, `AddObjectToAsset` onto that Renderer Data, append
   `m_RendererFeatures` and `m_RendererFeatureMap`, save.

The Feature is a sub-asset. Removing the plugin without cleaning Renderer Data
leaves a missing-script slot; delete it by hand.

## State

`DrawModePlusRuntimeState` is a static editor/runtime bridge (no SO):

- `CurrentMode`
- `DepthMeter` → global `_DepthMeter`
- `ForceMirror` / `ReflectionRoughness` → `_DrawModePlusReflectionRoughness`
  (0 when Force Mirror is on)

`CustomDrawModeInitializer` also sets `_DrawModeIsForward` for shader graphs.

## Resource lookup

`ResourceFinder.GetPluginPath()` finds `DrawModePlus.asmdef` through
`AssetDatabase.FindAssets`. That works for an `Assets/` copy and for a UPM
`Packages/com.mlsmoon.drawmodeplus` install.

Textures load from `{plugin}/Arts/Textures/...`.
UV0 uses `Common/ColorUV.png`.

## Assemblies

| Asmdef | Role |
|---|---|
| `DrawModePlus.Runtime` | Feature, state, passes. References URP. |
| `DrawModePlus` | Editor only. References the runtime asmdef. |
