#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

float3 IncomingLight(Surface surface, Light light)
{
    // Clamp the dot product to 0 via the saturate function - this is to "normalize" the values for a surfce that's lit.
    return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting(Surface surface, BRDF brdf, Light light)
{
    return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}

float3 GetLighting(Surface surface, BRDF brdf)
{
    // Obsolete now - but left here as reference
    // Allow for albedo - how much light is diffusely reflected by a surface :)
    // return surface.normal.y * surface.color;

    // Obsolete now - but left here as reference
    // We want to take into consideration the Directional Light
    // return GetLighting(surface, GetDirectionalLight());

    float3 color = 0.0;
    for (int i = 0; i < GetDirectionalLightCount(); i++)
    {
        color += GetLighting(surface, brdf, GetDirectionalLight(i));
    }
    return color;
}

#endif
