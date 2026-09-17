# DrawModePlus: Adding a Mode

Do these steps in order. Skip Unity Test scripts.

## 1. Enum

Add a value to `DrawModePlusMode` in `Runtime/DrawModePlusRuntimeState.cs`.
If it is fullscreen, also list it in `IsFullscreenMode`.

## 2. Registry

Add the value to `OrderedModes` and `DisplayNames` in
`Editor/DrawModePlusModeRegistry.cs`. The SceneView section stays
`DrawModePlusMLS`.

## 3. Handler

Create `Editor/DrawModes/YourModeDrawMode.cs`:

```csharp
namespace DrawModePlusMLS.Editor
{
    public class YourModeDrawMode : CustomDrawModeBase
    {
        public override void OnInitialize()
        {
            Mode = global::DrawModePlusMLS.DrawModePlusMode.YourMode;
            base.OnInitialize();
        }
    }
}
```

Register a `new YourModeDrawMode()` in `CustomDrawModeInitializer`.

## 4. Pass and shader

- Fullscreen blit: reuse `FullscreenDebugPass` and add a shader +
  `GetFullscreenMaterial` case.
- Scene override: subclass `SceneObjectDebugPass`, clear, then
  `DrawSceneObjects` with an override material.
- Deferred buffer: reuse `DeferredDebugView` pass indices if possible.
- New LightMode: document it in the use skill. Do not hard-code host
  shader names.

Wire the mode in `DrawModePlusRendererFeature.AddRenderPasses`.
Resolve shaders with `Shader.Find("DrawModePlus/...")`.

## 5. Docs

- English README feature table
- `Docs/README.zh-Hans.md` translation
- Use-skill `references/mode-catalog.md`
- `CHANGELOG.md`

## 6. Verify

SceneView dropdown, control panel, Forward vs Deferred, and mode `None`
cleanup. Confirm the new assemblies are newer than the sources before Play.
