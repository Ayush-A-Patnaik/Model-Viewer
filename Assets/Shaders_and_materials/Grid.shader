Shader "Unlit/Grid"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.1, 0.1, 0.1, 1)
        _GridColor ("Grid Color", Color) = (0.45, 0.45, 0.45, 1)
        _MajorGridColor ("Major Grid Color", Color) = (0.6, 0.6, 0.6, 1)
        _GridSize ("Grid Size", Float) = 1
        _MajorGridSize ("Major Grid Size", Float) = 10
        _LineWidth ("Line Width", Float) = 0.02
        _MajorLineWidth ("Major Line Width", Float) = 0.05
        _FadeStart ("Fade Start", Float) = 20
        _FadeEnd ("Fade End", Float) = 80
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        //ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _BaseColor;
            float4 _GridColor;
            float4 _MajorGridColor;
            float _GridSize;
            float _MajorGridSize;
            float _LineWidth;
            float _MajorLineWidth;
            float _FadeStart;
            float _FadeEnd;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float GridLine(float coord, float width)
            {
                float f = abs(frac(coord - 0.5) - 0.5) / fwidth(coord);
                return 1.0 - saturate(f - width);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pos = i.worldPos.xz;

                float minorX = GridLine(pos.x / _GridSize, _LineWidth);
                float minorZ = GridLine(pos.y / _GridSize, _LineWidth);

                float majorX = GridLine(pos.x / _MajorGridSize, _MajorLineWidth);
                float majorZ = GridLine(pos.y / _MajorGridSize, _MajorLineWidth);

                float minor = max(minorX, minorZ);
                float major = max(majorX, majorZ);

                float gridMask = max (major, minor);
                float3 gridColor = lerp(_GridColor.rgb, _MajorGridColor.rgb, major);
                float3 finalColor = lerp(_BaseColor.rgb, gridColor, gridMask);

                float dist = distance(_WorldSpaceCameraPos.xz, pos);
                float fade = saturate(1 - (dist - _FadeStart) / (_FadeEnd - _FadeStart));

                float finalAlpha = lerp(_BaseColor.a, 1.0, gridMask) * fade;

                return float4(finalColor, finalAlpha);
                // float4 color = lerp(_GridColor, _MajorGridColor, major);
                // float alpha = max(minor, major);
                //
                // float dist = distance(_WorldSpaceCameraPos.xz, pos);
                // float fade = saturate(1 - (dist - _FadeStart) / (_FadeEnd - _FadeStart));
                //
                // return float4(color.rgb, alpha * fade);
            }
            ENDCG
        }
    }
}
