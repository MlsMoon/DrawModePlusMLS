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
    }
}
