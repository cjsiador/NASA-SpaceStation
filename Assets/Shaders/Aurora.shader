Shader "Custom/Aurora"
{
    Properties
    {
        _Color1 ("Base Color", Color) = (0, 1, 0.6, 1)
        _Color2 ("Tip Color", Color) = (0.2, 0.5, 1, 1)
        _Speed ("Wave Speed", Float) = 2.0
        _NoiseScale ("Noise Scale", Float) = 2.5
        _GlowIntensity ("Glow Intensity", Float) = 1.0
        _Height ("Ribbon Height", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend One One
        ZWrite Off
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color1;
            float4 _Color2;
            float _Speed;
            float _NoiseScale;
            float _GlowIntensity;
            float _Height;
            // float _Time;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            // Simple fake noise
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) + (c - a)* u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Animate vertical wave motion
                // float offset = sin((v.vertex.y * _NoiseScale) + _Time * _Speed) * 0.1;
                float offset = sin((v.vertex.y * _NoiseScale) + _Time.y * _Speed) * 0.1;
                float3 displaced = v.vertex.xyz;
                displaced.x += offset;

                o.pos = UnityObjectToClipPos(float4(displaced, 1));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Use UV.y as height factor
                float heightFactor = i.uv.y;

                // Animate noise flicker
                // float2 noiseUV = float2(i.worldPos.x * _NoiseScale, _Time * _Speed);
                float2 noiseUV = float2(i.worldPos.x * _NoiseScale, _Time.y * _Speed);

                float n = noise(noiseUV);

                // Blend colors from bottom to top
                float4 col = lerp(_Color1, _Color2, heightFactor + n * 0.2);
                col *= _GlowIntensity;

                // Fade out at top/bottom
                float alpha = smoothstep(0.1, 0.3, heightFactor) * (1.0 - heightFactor);
                col.a = alpha;

                return col;
            }
            ENDCG
        }
    }
}
