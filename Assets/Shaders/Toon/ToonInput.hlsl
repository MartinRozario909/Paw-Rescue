#ifndef PAW_TOON_INPUT_INCLUDED
#define PAW_TOON_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

// Every pass shares this CBUFFER so the material stays SRP Batcher compatible.
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _Cutoff;

    half4 _ShadowColor;
    half _ShadowThreshold;
    half _ShadowSoftness;
    half _AmbientStrength;

    half4 _SpecularColor;
    half _SpecularSize;
    half _SpecularSoftness;

    half4 _RimColor;
    half _RimSize;
    half _RimSoftness;

    half4 _OutlineColor;
    half _OutlineWidth;
CBUFFER_END

// Hard-edged step with a small anti-aliased transition.
half ToonStep(half value, half threshold, half softness)
{
    softness = max(softness, 0.001h);
    return smoothstep(threshold - softness, threshold + softness, value);
}

#endif
