using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer {

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    static Material errorMat;

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
        DrawUnsupportedShaders();
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
        var sortingSettings   = new SortingSettings(cam) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings   = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        ctx.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        ctx.DrawSkybox(cam);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        ctx.DrawRenderers(cullingResults, ref drawingSettings, ref  filteringSettings);
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

    void DrawUnsupportedShaders() {
        if (errorMat == null) {
            errorMat = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(cam)) {
            overrideMaterial = errorMat
        };
        for (int i = 1; i < legacyShaderTagIds.Length; i++) {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        ctx.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
}
