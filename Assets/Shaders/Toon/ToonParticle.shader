// Unlit toon particle: the soft particle texture is cut into a hard-edged
// shape and its brightness posterized into flat bands.
Shader "Paw Rescue/Toon Particle"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _ShadeColor("Inner Shade Color", Color) = (0.8, 0.75, 0.7, 1)

        _AlphaThreshold("Edge Threshold", Range(0, 1)) = 0.35
        _EdgeSoftness("Edge Softness", Range(0, 0.5)) = 0.03
        _ShadeThreshold("Inner Shade Threshold", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Pass
        {
            Name "ToonParticle"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadeColor;
                half _AlphaThreshold;
                half _EdgeSoftness;
                half _ShadeThreshold;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color       : COLOR;
                float2 texcoord   : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv         : TEXCOORD0;
                half4 color       : COLOR;
                half fogFactor    : TEXCOORD1;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half softness = max(_EdgeSoftness, 0.001h);

                // Shrinking the shape with particle alpha keeps fade-outs hard-edged.
                half shape = tex.a * input.color.a;
                half alpha = smoothstep(_AlphaThreshold - softness, _AlphaThreshold + softness, shape);
                half core = smoothstep(_ShadeThreshold - softness, _ShadeThreshold + softness, shape);

                half3 color = lerp(_ShadeColor.rgb, half3(1, 1, 1), core) * _BaseColor.rgb * input.color.rgb;
                color = MixFog(color, input.fogFactor);
                return half4(color, alpha * _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
