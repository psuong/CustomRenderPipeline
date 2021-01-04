using UnityEngine;
using UnityEngine.Rendering;

public class Shadows {

    struct ShadowedDirectionalLight {
        public int VisibleLightIndex;
    }

    const string BufferName = "Shadows";

    // NOTE: We can add more, but shadows add more draw call overhead.
    const int MaxShadowedDirectonalLightCount = 1;

    static int DirShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");

    ScriptableRenderContext context;
    CullingResults cullingResults;
    ShadowSettings shadowSettings;

    int shadowDirectionalLightCount;

    CommandBuffer buffer = new CommandBuffer { name = BufferName };
    ShadowedDirectionalLight[] ShadowedDirectionalLights = new ShadowedDirectionalLight[MaxShadowedDirectonalLightCount];

    public void SetUp(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings) {
        this.context = context;
        this.cullingResults = cullingResults;
        this.shadowSettings = shadowSettings;

        shadowDirectionalLightCount = 0;
    }

    public void ReserveDirectionalShadows(Light light, int visibleLightIndex) {
        // Check if the light can project a shadow and if the rendered elements are within bounds.
        if (shadowDirectionalLightCount < MaxShadowedDirectonalLightCount && 
            light.shadows != LightShadows.None && 
            light.shadowStrength > 0 && 
            cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds bounds)) {

            ShadowedDirectionalLights[shadowDirectionalLightCount++] = new ShadowedDirectionalLight {
                VisibleLightIndex = visibleLightIndex
            };
        };
    }

    public void Render() {
        if (shadowDirectionalLightCount > 0) {
            RenderDirectionalShadows();
        } else {
            buffer.GetTemporaryRT(
                DirShadowAtlasID, 1, 1,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

            // Tell the GPu to render the shadow map to a render texture instead of the camera.
            // We only need to store the shadows for infomation.
            buffer.SetRenderTarget(DirShadowAtlasID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

            buffer.ClearRenderTarget(true, false, Color.clear);
            ExecuteBuffer();
        }
    }

    public void CleanUp() {
        buffer.ReleaseTemporaryRT(DirShadowAtlasID);
        ExecuteBuffer();
    }

    void ExecuteBuffer() {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void RenderDirectionalShadows() {
        int atlasSize = (int)shadowSettings.DirectionalShadows.AtlasSize;
        buffer.GetTemporaryRT(
            DirShadowAtlasID, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

        buffer.ClearRenderTarget(true, false, Color.clear);

        buffer.BeginSample(BufferName);

        for (int i = 0; i < shadowDirectionalLightCount; i++) {
            RenderDirectionalShadows(i, atlasSize);
        }

        buffer.EndSample(BufferName);
        ExecuteBuffer();
    }
    
    void RenderDirectionalShadows(int index, int tileSize) {
        var light = ShadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, index);

        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.VisibleLightIndex, 
            0, 
            1, 
            Vector3.zero, 
            tileSize, 
            0f, 
            out Matrix4x4 viewMatrix, 
            out Matrix4x4 projMatrix, 
            out ShadowSplitData shadowSplitData);

        shadowSettings.splitData = shadowSplitData;
        buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }
}

