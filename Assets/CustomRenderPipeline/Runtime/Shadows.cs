using UnityEngine;
using UnityEngine.Rendering;

public class Shadows {

    struct ShadowedDirectionalLight {
        public int VisibleLightIndex;
    }

    const string BufferName = "Shadows";
    const int MaxShadowedDirectonalLightCount = 4; // NOTE: We can add more, but shadows add more draw call overhead.

    static int DirShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
    static int DirShadowMatricesID = Shader.PropertyToID("_DirectionalShadowMatrices");

    static Matrix4x4[] DirShadowMatrices = new Matrix4x4[MaxShadowedDirectonalLightCount];

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

    public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex) {
        // Check if the light can project a shadow and if the rendered elements are within bounds.
        if (shadowDirectionalLightCount < MaxShadowedDirectonalLightCount && 
            light.shadows != LightShadows.None && 
            light.shadowStrength > 0 && 
            cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds bounds)) {

            ShadowedDirectionalLights[shadowDirectionalLightCount] = new ShadowedDirectionalLight {
                VisibleLightIndex = visibleLightIndex
            };

            return new Vector2(light.shadowStrength, shadowDirectionalLightCount++);
        };

        return Vector2.zero;
    }

    public void Render() {
        if (shadowDirectionalLightCount > 0) {
            RenderDirectionalShadows();
        } else {
            buffer.GetTemporaryRT(
                DirShadowAtlasID, 1, 1,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

            // Tell the GPU to render the shadow map to a render texture instead of the camera.
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

        // TODO: Looks like I missed a step...
        buffer.SetRenderTarget(DirShadowAtlasID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, false, Color.clear);

        buffer.BeginSample(BufferName);
        ExecuteBuffer();

        int split = shadowDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;

        for (int i = 0; i < shadowDirectionalLightCount; i++) {
            RenderDirectionalShadows(i, split, tileSize);
        }

        buffer.EndSample(BufferName);
        ExecuteBuffer();
    }
    
    void RenderDirectionalShadows(int index, int split, int tileSize) {
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

        // SetTileViewport(index, split, tileSize);
        DirShadowMatrices[index] = ConvertToAtlasMatrix(
            projMatrix * viewMatrix, 
            SetTileViewport(index, split, tileSize), 
            split);

        buffer.SetGlobalMatrixArray(DirShadowMatricesID, DirShadowMatrices);
        buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }

    Vector2 SetTileViewport(int index, int split, float tileSize) {
        var offset = new Vector2(index % split, index / split);

        buffer.SetViewport(new Rect(
            offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
        ));

        return offset;
    }

    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split) {
        if (SystemInfo.usesReversedZBuffer) {
			m.m20 = -m.m20;
			m.m21 = -m.m21;
			m.m22 = -m.m22;
			m.m23 = -m.m23;
        }

		float scale = 1f / split;
		m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
		m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
		m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
		m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
		m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
		m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
		m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
		m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        return m;
    }
}

