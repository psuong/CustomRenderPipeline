using UnityEngine;
using UnityEngine.Rendering;

public class Shadows 
{
    struct ShadowedDirectionalLight
    {
        public int VisibleLightIndex;
    }

    static int    DirShadowAtlasID                 = Shader.PropertyToID("_DirectionalShadowAtlas");
    const  string BufferName                       = "Shadows";
    const  int    MaxShadowedDirectionalLightCount = 1;

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
        // TODO: Implement this
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer .Clear();
    }
}
