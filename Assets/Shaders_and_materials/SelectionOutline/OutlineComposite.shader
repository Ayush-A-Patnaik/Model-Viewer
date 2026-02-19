Shader "Custom/OutlineComposite"
{
    Properties
    {
        // _MainTex is set via MaterialPropertyBlock from the render pass
        _MainTex ("Mask Texture", 2D) = "black" {}
        _OutlineColor ("Outline Color", Color) = (1, 0.6, 0, 1)
        _OutlineThickness ("Outline Thickness", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "OutlineComposite"
            ZWrite Off
            ZTest LEqual
            // Blend so the outline composites on top of the scene
            // without wiping pixels that aren't on the outline ring
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Declared outside CBUFFER so MaterialPropertyBlock can set it
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                float4 _OutlineColor;
                float _OutlineThickness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                // We intentionally ignore mesh UVs — derived from clip pos instead
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Derive UV from clip space position — this is correct on all
                // platforms regardless of render target flip or fullscreenMesh quirks.
                // Clip space X and Y are in [-1, 1], we remap to [0, 1].
                float2 uv = OUT.positionHCS.xy / OUT.positionHCS.w;
                uv = uv * 0.5 + 0.5;

                // On D3D/Metal, the Y axis of render textures is flipped
                // relative to OpenGL convention. UNITY_UV_STARTS_AT_TOP handles this.
                #if UNITY_UV_STARTS_AT_TOP
                    uv.y = 1.0 - uv.y;
                #endif

                OUT.uv = uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Sample center — if we're inside the silhouette, discard
                float centerMask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;
                if (centerMask > 0.5)
                    return half4(0, 0, 0, 0);

                // Sample 8 neighbors to detect edge proximity
                float2 texelSize = _MainTex_TexelSize.xy * _OutlineThickness;

                float n = 0;
                n += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( texelSize.x,  0          )).r;
                n += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texelSize.x,  0          )).r;
                n += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 0,            texelSize.y)).r;
                n += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 0,           -texelSize.y)).r;
                n += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( texelSize.x,  texelSize.y)).r;
                n += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texelSize.x,  texelSize.y)).r;
                n += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( texelSize.x, -texelSize.y)).r;
                n += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texelSize.x, -texelSize.y)).r;

                if (n > 0)
                    return _OutlineColor;

                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}