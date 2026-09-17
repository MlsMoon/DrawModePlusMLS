# DrawModePlusMLS Example

Open `Demo.unity` after the plugin has compiled and injected
`DrawModePlusRendererFeature`.

## What is in here

- `Demo.unity` — sample objects for depth, normals, UV0, and stencil
- `Prefabs/NormalMap.prefab` — compare normal-map vs flat shading in Deferred
- Shared demo meshes, materials, and textures stay in `../Arts/` so the shaders
  and the UV0 atlas remain next to the plugin code

## How to use

1. Open the scene.
2. Select the SceneView Draw Mode dropdown → **DrawModePlusMLS**.
3. Cycle Depth, WorldNormal, UV0, and the Deferred GBuffer modes.
4. For TexelDensity, objects using only the bundled sample shaders stay gray
   unless those shaders expose `DrawModePlusTexelDensity`.

The example does not depend on a host game framework.
