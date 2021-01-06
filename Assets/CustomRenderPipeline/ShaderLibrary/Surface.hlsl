#ifndef CUSTOM_SURFACE_INLCUDED
#define CUSTOM_SURFACE_INLCUDED


struct Surface
{
    float3 position;
    float3 normal;
    float3 viewDirection;
    float3 color;
    float  alpha;
    float  metallic;
    float  smoothness;
};

#endif
