Shader "Custom/OutlineMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "OutlineMask"

            // Do NOT write to color or depth in the normal sense —
            // we just want a white silhouette in our mask render texture
            Cull Off
            ZWrite Off
            ZTest LEqual   // Always draw regardless of depth so occluded
                           // selected objects still appear in the mask.
                           // Change to LEqual if you want depth-correct outlines.

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Solid white — this becomes the silhouette mask
                return half4(1, 1, 1, 1);
            }
            ENDHLSL
        }
    }
}
