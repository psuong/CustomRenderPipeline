using UnityEngine;
using UnityEngine.Rendering;

public class Shadows 
{
    const string BufferName = "Shadows";

    CommandBuffer buffer = new CommandBuffer
    {
        name = BufferName
    };

    ScriptableRenderContext context;
    CullingResults cullingResults;
    ShadowSettings settings;

    public void SetUp(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
    {
        this.context        = context;
        this.cullingResults = cullingResults;
        this.settings       = settings;
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}
