#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BDRF.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

// Doesn't allow instancing support so we can get rid of this
//CBUFFER_START(UnityPerMaterial)
//    float4 _BaseColor;
//CBUFFER_END

// Must be declared in the global scope as all shaders can access these resources
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)    // Provide tiling/offset
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)     // Provide instanced colors
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)         // Provide cut off support
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)       // Rpvoide instancing for metallic property
    UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)     // Provide instancing for the smoothness property
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Varyings 
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : VAR_POSITION;
    float3 normalWS : VAR_NORMAL;
    float2 baseUV : VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Attributes 
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 baseUV : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 LitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap   = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 base      = baseMap * baseColor;

    #if defined(_CLIPPING)
    clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    #endif

    // Colour the normals and show it in the preview - obv we aren't going to use this ;)
    // base.rgb = input.normalWS;
    // base.rgb = abs(length(input.normalWS) - 1.0) * 10.0;
    // base.rgb = normalize(input.normalWS);

    Surface surface;
    surface.normal        = normalize(input.normalWS);
    surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    surface.color         = base.rgb;
    surface.alpha         = base.a;

    // Copy the metallic and smoothness property
    surface.metallic   = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
    surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Smoothness);

    #if defined(_PREMULTIPLY_ALPHA)
    BRDF brdf    = GetBRDF(surface, true);
    #else
    BRDF brdf    = GetBRDF(surface);
    #endif
    
    float3 color = GetLighting(surface, brdf);
    return float4(color, surface.alpha);
}

Varyings LitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    output.positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS   = TransformObjectToWorldNormal(input.normalOS);

    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    output.baseUV = input.baseUV * baseST.xy + baseST.zw;

    return output;
}

#endif
