#ifndef PAW_TOON_FORWARD_PASS_INCLUDED
#define PAW_TOON_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 texcoord   : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv         : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS    : TEXCOORD2;
    half fogFactor    : TEXCOORD3;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings ToonVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionWS = positionInputs.positionWS;
    output.normalWS = half3(normalInputs.normalWS);
    output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
    output.positionCS = positionInputs.positionCS;
    return output;
}

// Cel-shaded diffuse + specular for a single light.
half3 ToonLight(Light light, half3 albedo, half3 normalWS, half3 viewDirWS, bool stepAttenuation)
{
    half NdotL = dot(normalWS, light.direction);
    half lit = ToonStep(NdotL, _ShadowThreshold, _ShadowSoftness);

    half attenuation = light.distanceAttenuation * light.shadowAttenuation;
    // Snap attenuation into flat bands so shadows and light falloff stay graphic.
    attenuation = stepAttenuation ? ToonStep(attenuation, 0.5h, _ShadowSoftness) : floor(saturate(attenuation) * 3.0h + 0.5h) / 3.0h;
    lit *= attenuation;

    half3 halfDir = SafeNormalize(light.direction + viewDirWS);
    half NdotH = saturate(dot(normalWS, halfDir));
    half specular = ToonStep(NdotH, 1.0h - _SpecularSize, _SpecularSoftness) * lit * step(0.0001h, _SpecularSize);

    half3 diffuse = albedo * lerp(_ShadowColor.rgb, half3(1, 1, 1), lit);
    return (diffuse + specular * _SpecularColor.rgb) * light.color;
}

half4 ToonFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half4 baseSample = SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half alpha = Alpha(baseSample.a, _BaseColor, _Cutoff);
    half3 albedo = baseSample.rgb * _BaseColor.rgb;

    half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half4 shadowMask = half4(1, 1, 1, 1);

    // InputData is only needed by the Forward+ cluster light loop macros.
    InputData inputData = (InputData)0;
    inputData.positionWS = input.positionWS;
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
    Light mainLight = GetMainLight(shadowCoord, input.positionWS, shadowMask);

    half3 color = ToonLight(mainLight, albedo, normalWS, viewDirWS, true);

    // Rim light only on the lit side, keeps silhouettes readable.
    half NdotL = dot(normalWS, mainLight.direction);
    half rim = 1.0h - saturate(dot(viewDirWS, normalWS));
    rim = ToonStep(rim, 1.0h - _RimSize, _RimSoftness) * ToonStep(NdotL, _ShadowThreshold, _ShadowSoftness);
    color += rim * _RimColor.rgb * _RimColor.a * mainLight.color;

    color += albedo * SampleSH(normalWS) * _AmbientStrength;

#if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
    #if USE_CLUSTER_LIGHT_LOOP
    [loop] for (uint dirLightIndex = 0; dirLightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); dirLightIndex++)
    {
        Light dirLight = GetAdditionalLight(dirLightIndex, input.positionWS, shadowMask);
        color += ToonLight(dirLight, albedo, normalWS, viewDirWS, true);
    }
    #endif

    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, input.positionWS, shadowMask);
        color += ToonLight(light, albedo, normalWS, viewDirWS, false);
    LIGHT_LOOP_END
#endif

    color = MixFog(color, InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor));
    return half4(color, alpha);
}

#endif
