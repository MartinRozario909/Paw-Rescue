#ifndef PAW_TOON_OUTLINE_PASS_INCLUDED
#define PAW_TOON_OUTLINE_PASS_INCLUDED

struct OutlineAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 texcoord   : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct OutlineVaryings
{
    float2 uv         : TEXCOORD0;
    half fogFactor    : TEXCOORD1;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

// Inverted hull: push vertices out along the normal in clip space so the
// outline keeps a constant on-screen width regardless of distance.
OutlineVaryings OutlineVertex(OutlineAttributes input)
{
    OutlineVaryings output = (OutlineVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);
    float3 normalCS = TransformWorldToHClipDir(TransformObjectToWorldNormal(input.normalOS));
    float2 offset = normalize(normalCS.xy + 1e-5);
    float aspect = _ScreenParams.y / _ScreenParams.x;
    offset.x *= aspect;
    positionCS.xy += offset * _OutlineWidth * 0.01 * positionCS.w;

    output.positionCS = positionCS;
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.fogFactor = ComputeFogFactor(positionCS.z);
    return output;
}

half4 OutlineFragment(OutlineVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);

    #if defined(_ALPHATEST_ON)
        Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    #endif

    half3 color = MixFog(_OutlineColor.rgb, input.fogFactor);
    return half4(color, 1);
}

#endif
