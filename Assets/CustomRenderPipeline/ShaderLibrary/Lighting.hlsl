#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

float3 IncomingLight(Surface surface, Light light)
{
    // Clamp the dot product to 0 via the saturate function - this is to "normalize" the values for a surfce that's lit.
    return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting(Surface surface, Light light)
{
    return IncomingLight(surface, light) * surface.color;
}

float3 GetLighting(Surface surface)
{
    // Allow for albedo - how much light is diffusely reflected by a surface :)
    // return surface.normal.y * surface.color;

    // We want to take into consideration the Directional Light
    return GetLighting(surface, GetDirectionalLight());
}

#endif
