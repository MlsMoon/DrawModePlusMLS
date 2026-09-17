# DrawModePlus: Texel Density Host Pass

`TexelDensityDebugPass` does two draws after clearing to black:

1. All scene objects with `DrawModePlus/FlatGray` (unintegrated = gray).
2. Objects whose shaders contain `LightMode = "DrawModePlusTexelDensity"`.

Unity only includes the second draw if that tag exists. There is no automatic
rewrite of host shaders.

## Required tag

```hlsl
Pass
{
    Name "DrawModePlusTexelDensity"
    Tags { "LightMode" = "DrawModePlusTexelDensity" }

    HLSLPROGRAM
    #pragma vertex Vert
    #pragma fragment Frag

    half4 Frag(Varyings input) : SV_Target
    {
        float2 uv = input.uv0;
        float2 ddxUV = ddx(uv);
        float2 ddyUV = ddy(uv);
        float texelDensity = /* texels per world unit from your main map */;
        float normalized = texelDensity / 512.0;
        return half4(normalized, normalized, normalized, 1.0);
    }
    ENDHLSL
}
```

Use the mesh's primary UV and the main albedo `TexelSize`. The 512 reference
matches the SceneView legend. Change the divisor only if the host art Bible
uses another target density, and document it in the host project.

## Legend

| Color | Meaning |
|---|---|
| Red ≤128 | Too low — blurry |
| Orange 256 | Below reference |
| Green 512 | Reference |
| Cyan 1024 | Above reference |
| Blue ≥2048 | Likely wasted memory |
| Gray | Shader has no tag |

## Pitfalls

- Integrating only one shader leaves the rest gray. That is expected.
- Do not output albedo or world position in this pass.
- Transparent queues are drawn, but the override still uses the tagged pass.
- The bundled sample shaders may not include the tag. Gray in `Example/Demo.unity`
  does not mean the plugin is broken.
