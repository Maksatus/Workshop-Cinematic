Shader "FakeGameplay/Particles/FlipbookBlend"
{
    // URP particle shader with crossfade blending between flipbook frames.
    //
    // SETUP в Particle System:
    //   Texture Sheet Animation → включить, задать Columns/Rows
    //   Renderer → Custom Vertex Streams:
    //     Position, Normal, Color, UV, UV2, AnimBlend  (для blend-режима)
    //     Position, Normal, Color, UV                  (без blend)
    //
    // BLEND MODE PRESETS (SrcBlend / DstBlend):
    //   Alpha:    SrcAlpha (5)        OneMinusSrcAlpha (10)
    //   Additive: SrcAlpha (5)        One (1)
    //   Premul:   One (1)             OneMinusSrcAlpha (10)

    Properties
    {
        [HDR] _Color ("Tint", Color) = (1,1,1,1)
        _MainTex ("Flipbook Atlas", 2D) = "white" {}

        [Space(8)]
        [Toggle(_FLIPBOOK_BLENDING)] _FlipbookBlending ("Flipbook Blend", Float) = 1

        [Space(8)]
        [Header(Blend Mode)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10
        [Toggle] _ZWrite ("ZWrite", Float) = 0

        [Space(8)]
        [Header(Soft Particles)]
        [Toggle(_SOFT_PARTICLES)] _SoftParticles ("Soft Particles", Float) = 0
        _SoftParticlesNearFadeDistance ("Near Fade Distance", Float) = 0.0
        _SoftParticlesFarFadeDistance  ("Far Fade Distance",  Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
        }

        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        Cull Off

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #pragma shader_feature_local _FLIPBOOK_BLENDING
            #pragma shader_feature_local _SOFT_PARTICLES

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #if defined(_SOFT_PARTICLES)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #endif

            // ─── Textures ────────────────────────────────────────────────
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // ─── Uniforms ─────────────────────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                float  _SoftParticlesNearFadeDistance;
                float  _SoftParticlesFarFadeDistance;
            CBUFFER_END

            // ─── Vertex Input ─────────────────────────────────────────────
            // UV / UV2 / AnimBlend — данные приходят из Particle System
            // через Custom Vertex Streams, не вычисляются в шейдере.
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
            #if defined(_FLIPBOOK_BLENDING)
                float4 texcoord   : TEXCOORD0; // xy = текущий кадр, zw = следующий кадр
                float  blendFactor: TEXCOORD1; // коэффициент из Particle System
            #else
                float2 texcoord   : TEXCOORD0;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // ─── Fragment Input ───────────────────────────────────────────
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4  color      : COLOR;
                float2 uvCurrent  : TEXCOORD0;
            #if defined(_FLIPBOOK_BLENDING)
                float2 uvNext     : TEXCOORD1;
                float  blend      : TEXCOORD2;
            #endif
            #if defined(_SOFT_PARTICLES)
                float4 positionNDC: TEXCOORD3;
            #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ─── Vert ─────────────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Vertex color из Particle System (color over lifetime, etc.) × material tint
                OUT.color = IN.color * _Color;

                // UV уже в пространстве атласа — Particle System сам посчитал тайлинг кадра.
                // _MainTex_ST позволяет добавить дополнительный сдвиг через инспектор, если надо.
            #if defined(_FLIPBOOK_BLENDING)
                OUT.uvCurrent = TRANSFORM_TEX(IN.texcoord.xy, _MainTex);
                OUT.uvNext    = TRANSFORM_TEX(IN.texcoord.zw, _MainTex);
                OUT.blend     = IN.blendFactor;
            #else
                OUT.uvCurrent = TRANSFORM_TEX(IN.texcoord.xy, _MainTex);
            #endif

            #if defined(_SOFT_PARTICLES)
                // NDC позиция нужна для чтения depth-буфера в fragment
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionNDC = vertexInput.positionNDC;
            #endif

                return OUT;
            }

            // ─── Frag ─────────────────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvCurrent);

            #if defined(_FLIPBOOK_BLENDING)
                half4 colNext = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvNext);
                col = lerp(col, colNext, IN.blend);
            #endif

                col *= IN.color;

            #if defined(_SOFT_PARTICLES)
                // Fade частицы там, где она пересекает непрозрачную геометрию.
                // Требует Depth Texture в URP Asset (Depth Texture → Enabled).
                float2 screenUV   = IN.positionNDC.xy / IN.positionNDC.w;
                float sceneDepth  = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float partDepth   = IN.positionNDC.w;
                float softFade    = saturate(
                    (sceneDepth - partDepth - _SoftParticlesNearFadeDistance) /
                    max(_SoftParticlesFarFadeDistance - _SoftParticlesNearFadeDistance, 1e-5)
                );
                col.a *= softFade;
            #endif

                return col;
            }

            ENDHLSL
        }
    }

    FallBack Off
}
