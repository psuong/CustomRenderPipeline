using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer {

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    /**
     * We want to see the cmd buffer in the profiler.
     */
    const string BufferName = "Render Camera";

    CommandBuffer buffer = new CommandBuffer() { name = BufferName };
    ScriptableRenderContext ctx;
    Camera cam;
    CullingResults cullingResults; // We want to figure out what can be rendered

    public void Render(ScriptableRenderContext ctx, Camera cam) {
        this.ctx = ctx;
        this.cam = cam;

        if (!Cull()) {
            return;
        }

        SetUp();
        DrawVisibleGeometry();
        // Submit the previous cmd to the render queue
        Submit();
    }

    void SetUp() {
        /**
         * Will allow for correct camera setup and clearing. If we didn't do this, we would have a DrawGL render cmd,
         * which draws a full size quad. We should see a Clear(color + z + stencil).
         */
        ctx.SetupCameraProperties(cam);
        buffer.ClearRenderTarget(true, true, Color.clear);
        buffer.BeginSample(BufferName);
        ExecuteBuffer();
    }

    void DrawVisibleGeometry() {
        var sortingSettings = new SortingSettings(cam);
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filterSettings = new FilteringSettings(RenderQueueRange.all);

        ctx.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);

        ctx.DrawSkybox(cam);
    }

    void Submit() {
        buffer.EndSample(BufferName);
        ExecuteBuffer();
        ctx.Submit();
    }

    void ExecuteBuffer() {
        ctx.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    bool Cull() {
        if (cam.TryGetCullingParameters(out var p)) {
            cullingResults = ctx.Cull(ref p);
            return true;
        }
        return false;
    }
}
