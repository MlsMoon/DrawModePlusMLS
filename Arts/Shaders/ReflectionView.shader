Shader "DrawModePlus/ReflectionView"
{
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ReflectionView"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Lighting.hlsl 先带 BRDFData / DebuggingCommon，禁止单独 include GlobalIllumination.hlsl。
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float _DrawModePlusReflectionRoughness;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 reflectVector = reflect(-viewDirectionWS, normalWS);
                half perceptualRoughness = saturate(_DrawModePlusReflectionRoughness);
                half3 color = GlossyEnvironmentReflection(
                    reflectVector,
                    input.positionWS,
                    perceptualRoughness,
                    1.0h,
                    GetNormalizedScreenSpaceUV(input.positionCS));
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
