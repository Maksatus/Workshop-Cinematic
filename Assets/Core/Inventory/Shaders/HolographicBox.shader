Shader "Custom/HolographicBox"
{
    Properties
    {
        _Color          ("Color",          Color)         = (0.2, 0.85, 0.75, 1.0)
        _FaceAlpha      ("Face Alpha",     Range(0, 1))   = 0.15
        _EdgeIntensity  ("Edge Opacity",   Range(0, 1))   = 1.0
        _EdgeWidthWS    ("Edge Width (m)", Range(0, 2))   = 0.05
        _BackFaceMult   ("Back Face Dim",  Range(0, 1))   = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Overlay"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "HolographicForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            //ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float  _FaceAlpha;
                float  _EdgeIntensity;
                float  _EdgeWidthWS;
                float  _BackFaceMult;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN, bool isFront : SV_IsFrontFace) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float2 uv = IN.uv;

                // World-space metres per UV unit, computed per screen quad.
                // This accounts for non-uniform scale on any face automatically.
                float wPerUV_U = length(ddx(IN.positionWS)) / max(abs(ddx(uv.x)), 1e-5);
                float wPerUV_V = length(ddy(IN.positionWS)) / max(abs(ddy(uv.y)), 1e-5);

                // Convert constant world-space edge width to UV-space thresholds.
                float threshU = _EdgeWidthWS / wPerUV_U;
                float threshV = _EdgeWidthWS / wPerUV_V;

                float distU = min(uv.x, 1.0 - uv.x);
                float distV = min(uv.y, 1.0 - uv.y);

                float maskU = 1.0 - smoothstep(0.0, threshU, distU);
                float maskV = 1.0 - smoothstep(0.0, threshV, distV);
                float edgeMask = max(maskU, maskV);

                float alpha = lerp(_FaceAlpha, _EdgeIntensity, edgeMask);

                if (!isFront) alpha *= _BackFaceMult;

                return half4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
