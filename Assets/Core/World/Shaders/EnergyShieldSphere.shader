Shader "Core/World/EnergyShieldSphere"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.08, 0.45, 1.0, 0.18)
        _FresnelColor ("Fresnel Color", Color) = (0.25, 0.9, 1.0, 1.0)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0

        _HexTex ("Pattern (Hex/Noise)", 2D) = "white" {}
        _PatternTiling ("Pattern Tiling", Range(0.5, 20.0)) = 8.0
        _PatternSpeed ("Pattern Speed", Range(0.0, 5.0)) = 0.8

        _PulseSpeed ("Pulse Speed", Range(0.0, 8.0)) = 2.5
        _PulseStrength ("Pulse Strength", Range(0.0, 2.0)) = 0.5

        _DistortStrength ("Distort Strength", Range(0.0, 0.25)) = 0.035
        _Alpha ("Alpha", Range(0.0, 1.0)) = 0.65
        _EffectStrength ("Effect Strength", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _HexTex;
            float4 _HexTex_ST;

            float4 _BaseColor;
            float4 _FresnelColor;
            float _RimPower;

            float _PatternTiling;
            float _PatternSpeed;
            float _PulseSpeed;
            float _PulseStrength;
            float _DistortStrength;
            float _Alpha;
            float _EffectStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);

                // Subtle breathing deformation to make shield feel alive.
                float wave = sin(_Time.y * _PulseSpeed + (worldPos.x + worldPos.y + worldPos.z) * 2.2);
                worldPos += worldNormal * wave * _DistortStrength;

                o.pos = UnityWorldToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _HexTex);
                o.worldPos = worldPos;
                o.worldNormal = normalize(worldNormal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float ndv = saturate(dot(i.worldNormal, viewDir));
                float fresnel = pow(1.0 - ndv, _RimPower);

                float2 pUV = i.uv * _PatternTiling;
                pUV += float2(_Time.y * _PatternSpeed, _Time.y * _PatternSpeed * 0.37);

                float pattern = tex2D(_HexTex, pUV).r;
                float pulse = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed * 1.35 + pattern * 6.2831);
                float energy = saturate(pattern * 0.75 + pulse * _PulseStrength);

                float3 col = _BaseColor.rgb;
                col += _FresnelColor.rgb * fresnel;
                col += _FresnelColor.rgb * energy * 0.45;
                col *= _EffectStrength;

                float alpha = _BaseColor.a * _Alpha;
                alpha += fresnel * 0.55;
                alpha += energy * 0.2;
                alpha = saturate(alpha) * _EffectStrength;

                return fixed4(col, alpha);
            }
            ENDCG
        }
    }

    Fallback Off
}
