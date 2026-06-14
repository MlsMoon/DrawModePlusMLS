Shader "DrawModePlus/DeferredDebugView"
{
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Pass
        {
            Name "MaterialAOCapture"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCapture

            TEXTURE2D_X_HALF(_GBuffer1);
            SamplerState my_point_clamp_sampler;

            half4 FragCapture(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half ao = SAMPLE_TEXTURE2D_X_LOD(_GBuffer1, my_point_clamp_sampler, uv, 0).a;
                return half4(ao, ao, ao, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "BaseColorCapture"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragBaseColorCapture

            TEXTURE2D_X_HALF(_GBuffer0);
            SamplerState my_point_clamp_sampler;

            half4 FragBaseColorCapture(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half3 baseColor = SAMPLE_TEXTURE2D_X_LOD(_GBuffer0, my_point_clamp_sampler, uv, 0).rgb;
                return half4(baseColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DeferredDebugComposite"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragComposite

            TEXTURE2D_X(_DrawModePlusMaterialAODebugTexture);
            SAMPLER(sampler_DrawModePlusMaterialAODebugTexture);

            half4 FragComposite(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half3 debugColor = SAMPLE_TEXTURE2D_X(_DrawModePlusMaterialAODebugTexture, sampler_DrawModePlusMaterialAODebugTexture, uv).rgb;
                return half4(debugColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "MetallicCapture"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragMetallicCapture

            TEXTURE2D_X_HALF(_GBuffer0);
            TEXTURE2D_X_HALF(_GBuffer1);
            SamplerState my_point_clamp_sampler;

            float UnpackMaterialFlags(float packedMaterialFlags)
            {
                return floor(packedMaterialFlags * 255.0 + 0.5);
            }

            half MetallicFromReflectivity(half reflectivity)
            {
                return (reflectivity - 0.04h) / 0.96h;
            }

            half4 FragMetallicCapture(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half4 gbuffer0 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer0, my_point_clamp_sampler, uv, 0);
                half3 packedSpecular = SAMPLE_TEXTURE2D_X_LOD(_GBuffer1, my_point_clamp_sampler, uv, 0).rgb;
                float materialFlags = UnpackMaterialFlags(gbuffer0.a);
                float specularSetupFlag = fmod(floor(materialFlags / 8.0), 2.0);

                half metallic = 0.0h;
                if (specularSetupFlag < 0.5)
                    metallic = saturate(MetallicFromReflectivity(packedSpecular.r));

                return half4(metallic, metallic, metallic, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "RoughnessCapture"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragRoughnessCapture

            TEXTURE2D_X_HALF(_GBuffer2);
            SamplerState my_point_clamp_sampler;

            half4 FragRoughnessCapture(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half smoothness = SAMPLE_TEXTURE2D_X_LOD(_GBuffer2, my_point_clamp_sampler, uv, 0).a;
                half roughness = saturate(1.0h - smoothness);
                return half4(roughness, roughness, roughness, 1.0);
            }
            ENDHLSL
        }
    }
}
