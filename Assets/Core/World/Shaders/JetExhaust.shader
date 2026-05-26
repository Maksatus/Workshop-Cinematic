Shader "Core/World/JetExhaust"
{
    Properties
    {
        _CoreColor ("Core Color", Color) = (0.45, 0.8, 1.0, 1.0)
        _OuterColor ("Outer Color", Color) = (0.1, 0.35, 1.0, 1.0)
        _Intensity ("Intensity", Range(0.0, 8.0)) = 2.5
        _Alpha ("Alpha", Range(0.0, 2.0)) = 0.9

        _MainTex ("Shape Mask", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "gray" {}
        _NoiseTiling ("Noise Tiling", Range(0.5, 12.0)) = 3.0
        _NoiseScroll ("Noise Scroll", Range(0.0, 12.0)) = 4.0
        _DistortStrength ("Distort Strength", Range(0.0, 0.4)) = 0.08

        _PulseSpeed ("Pulse Speed", Range(0.0, 20.0)) = 8.0
        _PulseAmount ("Pulse Amount", Range(0.0, 1.0)) = 0.28
        _RotationSpeed ("Rotation Speed", Range(-20.0, 20.0)) = 4.0

        _EdgeSoftness ("Edge Softness", Range(0.1, 8.0)) = 2.4
        _LengthFade ("Length Fade", Range(0.2, 4.0)) = 1.15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;

            float4 _CoreColor;
            float4 _OuterColor;
            float _Intensity;
            float _Alpha;
            float _NoiseTiling;
            float _NoiseScroll;
            float _DistortStrength;
            float _PulseSpeed;
            float _PulseAmount;
            float _RotationSpeed;
            float _EdgeSoftness;
            float _LengthFade;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float3 localPos : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float uvV = saturate(v.uv.y);
                float displacementMask = 1.0 - uvV;
                float2 noiseUV = v.uv * _NoiseTiling + float2(0.0, _Time.y * _NoiseScroll);
                float noise = tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0)).r * 2.0 - 1.0;

                float3 displaced = v.vertex.xyz;
                displaced.xy += v.normal.xy * noise * _DistortStrength * displacementMask;
                displaced.xy += v.normal.xy * (sin(_Time.y * _PulseSpeed + v.uv.y * 12.0) * _PulseAmount * 0.08) * displacementMask;

                float angle = _Time.y * _RotationSpeed;
                float s = sin(angle);
                float c = cos(angle);
                float x = displaced.x;
                float z = displaced.z;
                displaced.x = x * c - z * s;
                displaced.z = x * s + z * c;

                o.pos = UnityObjectToClipPos(float4(displaced, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(noiseUV, _NoiseTex);
                o.localPos = displaced;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float shape = tex2D(_MainTex, i.uv).r;
                float noiseA = tex2D(_NoiseTex, i.noiseUV).r;
                float noiseB = tex2D(_NoiseTex, i.noiseUV * 1.75 + float2(0.0, _Time.y * (_NoiseScroll * 0.45))).g;
                float turbulence = saturate((noiseA * 0.65 + noiseB * 0.35) * 1.25);

                float radial = 1.0 - abs(i.uv.x * 2.0 - 1.0);
                radial = pow(saturate(radial), _EdgeSoftness);

                float lengthMask = saturate(i.uv.y * _LengthFade);
                lengthMask = saturate(lengthMask * (1.0 - smoothstep(0.85, 1.0, i.uv.y)));

                float pulse = 1.0 + sin(_Time.y * _PulseSpeed + i.uv.y * 15.0) * _PulseAmount;
                float jet = shape * radial * lengthMask * turbulence * pulse;

                float3 col = lerp(_OuterColor.rgb, _CoreColor.rgb, radial);
                col *= jet * _Intensity;

                float alpha = jet * _Alpha;
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }

    Fallback Off
}
