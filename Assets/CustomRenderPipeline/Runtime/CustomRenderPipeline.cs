using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline 
{

    CameraRenderer renderer = new CameraRenderer();
    bool useDynamicBatching, useGPUInstancing;
    ShadowSettings shadowSettings;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, 
        ShadowSettings shadowSettings) 
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing   = useGPUInstancing;
        this.shadowSettings     = shadowSettings;

        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity            = true;
    }

    // This is similar to the UniversalRP, you can render different cameras in different ways.
    protected override void Render(ScriptableRenderContext context, Camera[] cameras) 
    {
        foreach (var cam in cameras) 
        {
            renderer.Render(context, cam, useDynamicBatching, useGPUInstancing, shadowSettings);
        }
    }
}
