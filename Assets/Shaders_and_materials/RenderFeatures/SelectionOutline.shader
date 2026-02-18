Shader "Hidden/SelectionOutline"
{
    Properties
    {
        _SourceTex ("Source", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0.6,0,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "SelectionOutline"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Only depend on Core.hlsl (stable across URP versions)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SourceTex);
            SAMPLER(sampler_SourceTex);

            float4 _OutlineColor;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Fullscreen triangle (no mesh needed)
            Varyings Vert(Attributes v)
            {
                Varyings o;

                float2 pos = float2(
                    (v.vertexID == 2) ? 3.0 : -1.0,
                    (v.vertexID == 1) ? 3.0 : -1.0
                );

                o.positionHCS = float4(pos, 0.0, 1.0);
                o.uv = 0.5 * (pos + 1.0);

                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                // We don't even need to sample the source yet.
                // Stencil test already isolated the pixels we want.
                return _OutlineColor;
            }

            ENDHLSL
        }
    }
}
