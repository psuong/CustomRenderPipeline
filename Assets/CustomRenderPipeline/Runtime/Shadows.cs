using UnityEngine;
using UnityEngine.Rendering;

public class Shadows 
{
    // TODO: Implement sampling shadows
    struct ShadowedDirectionalLight
    {
        public int VisibleLightIndex;
    }

    static int    DirectionalShadowAtlasID         = Shader.PropertyToID("_DirectionalShadowAtlas");
    const  string BufferName                       = "Shadows";
    const  int    MaxShadowedDirectionalLightCount = 4;

    ShadowedDirectionalLight[] shadowedDirectionalLights = 
        new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];

    CommandBuffer buffer = new CommandBuffer
    {
        name = BufferName
    };

    ScriptableRenderContext context;
    CullingResults          cullingResults;
    ShadowSettings          settings;
    int                     shadowedDirectionalLightCount;

    public void SetUp(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
    {
        this.context        = context;
        this.cullingResults = cullingResults;
        this.settings       = settings;

        shadowedDirectionalLightCount = 0;
    }

    public void CleanUp()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            buffer.ReleaseTemporaryRT(DirectionalShadowAtlasID);
            ExecuteBuffer();
        }
    }

    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (shadowedDirectionalLightCount < MaxShadowedDirectionalLightCount && 
            light.shadowStrength > 0f && 
            light.shadows != LightShadows.None &&
            cullingResults.GetShadowCasterBounds(visibleLightIndex, out var bounds))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount++] = new ShadowedDirectionalLight 
            {
                VisibleLightIndex = visibleLightIndex
            };
        }
    }

    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
    }

    void RenderDirectionalShadows()
    {
        // Create the temporary shadow atlas that we'll use as a render texture.
        var atlasSize = (int)settings._Directional.AtlasSize;
        buffer.GetTemporaryRT(DirectionalShadowAtlasID, atlasSize, atlasSize,
            32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);

        // The purpose of the render texture is to be used as a render texture - so we need to store it.
        buffer.SetRenderTarget(DirectionalShadowAtlasID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

        // Clear the render target so we can actually see everything
        buffer.ClearRenderTarget(true, false, Color.clear);
        buffer.BeginSample(BufferName);
        ExecuteBuffer();

        /**
         * To avoid super imposing each light onto each other, we want to split the atlas for four directional lights
         */
        var split    = shadowedDirectionalLightCount <= 1 ? 1 : 2;
        var tileSize = atlasSize / split;

        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }

        buffer.EndSample(BufferName);
        ExecuteBuffer();
    }

    void RenderDirectionalShadows(int index, int split, int tileSize) 
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        var shadowSettings             = new ShadowDrawingSettings(cullingResults, light.VisibleLightIndex);

        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.VisibleLightIndex, 0, 1,
            Vector3.zero, tileSize, 0f, 
            out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData);

        shadowSettings.splitData = splitData;
        SetTileViewPort(index, split, tileSize);
        buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);

        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }

    void SetTileViewPort(int index, int split, float tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}
