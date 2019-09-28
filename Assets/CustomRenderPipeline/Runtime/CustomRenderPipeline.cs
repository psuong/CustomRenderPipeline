using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline {

    CameraRenderer renderer = new CameraRenderer();

    // This is similar to the UniversalRP, you can render different cameras in different ways.
    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        foreach (var cam in cameras) {
            renderer.Render(context, cam);
        }
    }
}
