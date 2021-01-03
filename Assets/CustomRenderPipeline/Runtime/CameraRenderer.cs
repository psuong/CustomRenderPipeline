using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {

    static ShaderTagId UnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId LitShaderTagId   = new ShaderTagId("CustomLit");

    // We want to see the cmd buffer in the profiler.
    const string BufferName = "Render Camera";

    CommandBuffer buffer = new CommandBuffer() { name = BufferName };
    Lighting lighting    = new Lighting() {};

    ScriptableRenderContext ctx;
    Camera camera;
    CullingResults cullingResults; // We want to figure out what can be rendered

    public void Render(
        ScriptableRenderContext ctx, 
        Camera camera, 
        bool useDynamicBatching, 
        bool useGPUInstancing,
        ShadowSettings shadowSettings) {

        this.ctx = ctx;
        this.camera = camera;

        // Allow secondary cameras
        PrepareBuffer();
        // Allow UI Drawing in the scene view
        PrepareForSceneWindow();

        if (!Cull(shadowSettings.MaxDistance)) {
            return;
        }

        buffer.BeginSample(SampleName);
        ExecuteBuffer();

        // Do the lighting setup first to not affect the usual rendering.
        lighting.SetUp(ctx, cullingResults, shadowSettings);

        buffer.EndSample(SampleName);

        SetUp();

        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();

        // Clean up the lighting, which cleans up the shadows also.
        lighting.CleanUp();

        // Submit the previous cmd to the render queue
        Submit();
    }

    void SetUp() {
        /**
         * Will allow for correct camera setup and clearing. If we didn't do this, we would have a DrawGL render cmd,
         * which draws a full size quad. We should see a Clear(color + z + stencil).
         */
        ctx.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing) {
        var sortingSettings   = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings = new DrawingSettings(UnlitShaderTagId, sortingSettings) { 
            enableDynamicBatching = useDynamicBatching,
            enableInstancing      = useGPUInstancing 
        };
        drawingSettings.SetShaderPassName(1, LitShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        ctx.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        ctx.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        ctx.DrawRenderers(cullingResults, ref drawingSettings, ref  filteringSettings);
    }

    void Submit() {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        ctx.Submit();
    }

    void ExecuteBuffer() {
        ctx.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    bool Cull(float maxShadowDistance) {
        if (camera.TryGetCullingParameters(out var p)) {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = ctx.Cull(ref p);
            return true;
        }
        return false;
    }
}
