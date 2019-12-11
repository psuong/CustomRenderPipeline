using UnityEngine;
using UnityEngine.Rendering;

public class Lighting {

    static int DirLightColorID = Shader.PropertyToID("_DirectionalLightColor"),
               DirLightDirectionID = Shader.PropertyToID("_DirectionalLightDirection");
    const string BufferName = "Lighting";

    CommandBuffer buffer = new CommandBuffer {
        name = BufferName
    };

    public void SetUp(ScriptableRenderContext ctx) {
        buffer.BeginSample(BufferName);
        SetUpDirectionalLights();
        buffer.EndSample(BufferName);
        ctx.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    private void SetUpDirectionalLights() {
        var light = RenderSettings.sun;

        // Set up the light's color, direction
        // The direction of the light is typically the negated forward
        buffer.SetGlobalVector(DirLightColorID, light.color.linear * light.intensity);
        buffer.SetGlobalVector(DirLightDirectionID, -light.transform.forward);
    }
}

