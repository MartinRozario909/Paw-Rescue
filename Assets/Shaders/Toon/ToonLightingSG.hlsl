// Custom Function helpers for the Toon Shader Graph.
// Shader Graph has no node for main light colour/shadows or for additional
// lights, so these functions fetch them from URP. The graph does the toon maths.
#ifndef PAW_TOON_LIGHTING_SG_INCLUDED
#define PAW_TOON_LIGHTING_SG_INCLUDED

// Lighting.hlsl is only included in the forward pass (Unlit graph with
// "Keep Lighting Variants"). Previews and other passes get fixed fallbacks.
#if defined(SHADERGRAPH_PREVIEW) || !defined(UNIVERSAL_LIGHTING_INCLUDED)
    #define PAW_TOON_NO_LIGHTING
#endif

void ToonMainLight_float(float3 PositionWS, float3 NormalWS, out float3 Direction, out float3 Color, out float ShadowAttenuation, out float3 Ambient)
{
#if defined(PAW_TOON_NO_LIGHTING)
    Direction = normalize(float3(0.5, 0.5, -0.3));
    Color = float3(1, 1, 1);
    ShadowAttenuation = 1;
    Ambient = float3(0.2, 0.2, 0.2);
#else
    float4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
    Light light = GetMainLight(shadowCoord, PositionWS, half4(1, 1, 1, 1));
    Direction = light.direction;
    Color = light.color;
    ShadowAttenuation = light.shadowAttenuation * light.distanceAttenuation;
    Ambient = SampleSH(NormalWS);
#endif
}

// Sums cel-shaded point/spot lights. Distance falloff is snapped into 3 flat bands.
void ToonAdditionalLights_float(float3 PositionWS, float3 NormalWS, float4 ScreenPosition, float Threshold, float Softness, out float3 Diffuse)
{
    Diffuse = float3(0, 0, 0);

#if !defined(PAW_TOON_NO_LIGHTING) && (defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX))
    float softness = max(Softness, 0.001);
    half4 shadowMask = half4(1, 1, 1, 1);

    // InputData is only needed by the Forward+ cluster light loop macros.
    InputData inputData = (InputData)0;
    inputData.positionWS = PositionWS;
    inputData.normalizedScreenSpaceUV = ScreenPosition.xy;

    #if USE_CLUSTER_LIGHT_LOOP
    [loop] for (uint dirLightIndex = 0; dirLightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); dirLightIndex++)
    {
        Light dirLight = GetAdditionalLight(dirLightIndex, PositionWS, shadowMask);
        float dirLit = smoothstep(Threshold - softness, Threshold + softness, dot(NormalWS, dirLight.direction));
        dirLit *= smoothstep(0.5 - softness, 0.5 + softness, dirLight.shadowAttenuation);
        Diffuse += dirLight.color * dirLit;
    }
    #endif

    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, PositionWS, shadowMask);
        float lit = smoothstep(Threshold - softness, Threshold + softness, dot(NormalWS, light.direction));
        float attenuation = saturate(light.distanceAttenuation * light.shadowAttenuation);
        attenuation = floor(attenuation * 3.0 + 0.5) / 3.0;
        Diffuse += light.color * lit * attenuation;
    LIGHT_LOOP_END
#endif
}

#endif
