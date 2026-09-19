# DrawModePlus: Host Integration (xx)

Fictional host **xx**. Replace names with the real project. Do not write a real
host's Prefab, shader, or Renderer names back into this plugin.

Plugin contracts stay in the parent skill and `references/texel-density.md`.
This file is only the host wiring shape.

## Host call sites

1. **Opt-in shaders** — On materials that should show density (for example
   `XxLit.shader` under `Assets/Shaders/Xx/`), add a pass tagged
   `LightMode = "DrawModePlusTexelDensity"`. Output grayscale normalized to
   512 texels/m. Do not add the tag to every host shader.
2. **Skip list** — Unlit, UI, and post-process shaders stay gray on purpose.
   Gray means "not integrated", not "low density".
3. **Renderer Data** — xx uses the pipeline default URP Renderer Data for
   SceneView. Do not inject `DrawModePlusRendererFeature` from host code; the
   plugin auto-injects on editor load.
4. **Game View** — Leave **Enable Game View** off unless xx needs the same
   debug view on Game cameras. Preview cameras never draw.

## Host glue

- Keep the LightMode pass in the **host** shader. Do not patch plugin
  `Arts/Shaders/` to special-case xx.
- Art-Bible density other than 512 texels/m belongs in the host pass divisor
  and a host-project note, not in this plugin.
- SceneView section name stays `DrawModePlusMLS`. Do not rename it from the host.

## Hard rules

- URP only. Do not add Built-in or HDRP paths.
- Do not add Unity Test scripts or test asmdefs.
- Plugin shaders stay generic. Host lighting shaders stay in the host.
- Plugin git commits are English Conventional Commits (`AGENTS.md`).
