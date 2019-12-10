using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline {

    CameraRenderer renderer = new CameraRenderer();
    private bool useDynamicBatching, useGPUInstancing;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher) {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing   = useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
    }

    // This is similar to the UniversalRP, you can render different cameras in different ways.
    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        foreach (var cam in cameras) {
            renderer.Render(context, cam, useDynamicBatching, useGPUInstancing);
        }
    }
}
