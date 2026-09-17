# Third-party assets

Plugin code is MIT. Sample content under `Arts/` and `Example/` may use
additional licenses. These files are for the demo scene and documentation
screenshots; they are not required to run the debug modes in a host project.

| Asset | Location | Notes |
|---|---|---|
| Lato Regular | `Arts/Fonts/Lato-Regular.ttf` | [SIL Open Font License](https://scripts.sil.org/OFL) |
| Suzanne | `Arts/Meshes/Suzanne.fbx` | Blender Foundation monkey mesh, used as a demo prop |
| UV sphere / color calibrator | `Arts/Meshes/` | Demo meshes shipped with the example |
| Coin PBR set | `Arts/Textures/Coins/` | Demo textures for texel-density / material inspection |
| Color checker EXR | `Arts/Textures/Color_checker.EXR` | Demo texture |
| ColorUV checker | `Arts/Textures/Common/ColorUV.png` | UV0 overlay atlas used by the plugin |

If you ship a slimmer package without the example, you may delete `Example/`,
`Arts/Meshes/`, `Arts/Materials/`, `Arts/Fonts/`, and unused demo textures.
Keep the shaders in `Arts/Shaders/` and `Arts/Textures/Common/ColorUV.png`.
