using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline {

    CameraRenderer renderer = new CameraRenderer();

    ShadowSettings shadows;

    bool useDynamicBatching;
    bool useGPUInstancing;

    public CustomRenderPipeline(
        bool useDynamicBatching, 
        bool useGPUInstancing, 
        bool useSRPBatcher,
        ShadowSettings shadows) {

        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing   = useGPUInstancing;
        this.shadows            = shadows;

        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity            = true;

    }

    // This is similar to the UniversalRP, you can render different cameras in different ways.
    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        foreach (var cam in cameras) {
            renderer.Render(context, cam, useDynamicBatching, useGPUInstancing);
        }
    }
}
