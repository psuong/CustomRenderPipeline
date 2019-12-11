using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {

    static ShaderTagId 
        UnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
        LitShaderTagId = new ShaderTagId("CustomLit");

    /**
     * We want to see the cmd buffer in the profiler.
     */
    const string BufferName = "Render Camera";

    private CommandBuffer buffer = new CommandBuffer() { name = BufferName };
    private Lighting lighting    = new Lighting() {};

    private ScriptableRenderContext ctx;
    private Camera cam;
    private CullingResults cullingResults; // We want to figure out what can be rendered

    public void Render(ScriptableRenderContext ctx, Camera cam, bool useDynamicBatching, bool useGPUInstancing) {
        this.ctx = ctx;
        this.cam = cam;

        // Allow secondary cameras
        PrepareBuffer();
        // Allow UI Drawing in the scene view
        PrepareForSceneWindow();

        if (!Cull()) {
            return;
        }

        SetUp();
        lighting.SetUp(ctx);
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        // Submit the previous cmd to the render queue
        Submit();
    }

    private void SetUp() {
        /**
         * Will allow for correct camera setup and clearing. If we didn't do this, we would have a DrawGL render cmd,
         * which draws a full size quad. We should see a Clear(color + z + stencil).
         */
        ctx.SetupCameraProperties(cam);
        CameraClearFlags flags = cam.clearFlags;
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? cam.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing) {
        var sortingSettings   = new SortingSettings(cam) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings = new DrawingSettings(UnlitShaderTagId, sortingSettings) { 
            enableDynamicBatching = useDynamicBatching,
            enableInstancing      = useGPUInstancing 
        };
        drawingSettings.SetShaderPassName(1, LitShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        ctx.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        ctx.DrawSkybox(cam);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        ctx.DrawRenderers(cullingResults, ref drawingSettings, ref  filteringSettings);
    }

    private void Submit() {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        ctx.Submit();
    }

    private void ExecuteBuffer() {
        ctx.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    private bool Cull() {
        if (cam.TryGetCullingParameters(out var p)) {
            cullingResults = ctx.Cull(ref p);
            return true;
        }
        return false;
    }

}
