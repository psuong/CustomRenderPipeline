using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    const string BufferName       = "Lighting";
    const int    MaxDirLightCount = 4;

    static int DirLightCountID      = Shader.PropertyToID("_DirectionalLightCount"),
               DirLightColorsID     = Shader.PropertyToID("_DirectionalLightColors"),
               DirLightDirectionsID = Shader.PropertyToID("_DirectionalLightDirections");

    static Vector4[] 
        DirLightColors     = new Vector4[MaxDirLightCount],
        DirLightDirections = new Vector4[MaxDirLightCount];

    CullingResults cullingResults;

    CommandBuffer buffer = new CommandBuffer 
    {
        name = BufferName
    };

    Shadows shadows = new Shadows();

    public void SetUp(ScriptableRenderContext ctx, CullingResults cullingResults, ShadowSettings shadowSettings) 
    {
        this.cullingResults = cullingResults;

        buffer.BeginSample(BufferName);

        // SetUpDirectionalLights() is obsolete since it only supports 1 light
        // SetUpDirectionalLights();
        
        // Set up our shadows 
        shadows.SetUp(ctx, cullingResults, shadowSettings);

        // Set up all visible lights we want to support - for now only Directional ones :)
        SetUpLights();
        shadows.Render();

        buffer.EndSample(BufferName);
        ctx   .ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetUpLights() 
    {
        var visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++) {
            var currentLight = visibleLights[i];

            if (currentLight.lightType == LightType.Directional) {
                SetUpDirectionLight(dirLightCount++, ref currentLight);

                if (dirLightCount >= MaxDirLightCount) {
                    break;
                }
            }
        }

        buffer.SetGlobalInt(DirLightCountID, dirLightCount);
        buffer.SetGlobalVectorArray(DirLightColorsID, DirLightColors);
        buffer.SetGlobalVectorArray(DirLightDirectionsID, DirLightDirections);
    }

    void SetUpDirectionLight(int index, ref VisibleLight light) 
    {
        DirLightColors[index]     = light.finalColor;
        DirLightDirections[index] = -light.localToWorldMatrix.GetColumn(2);

        shadows.ReserveDirectionalShadows(light.light, index);
    }

    /*
    [System.Obsolete("Only supports 1 light, does not work if we have multiple lights")]
    void SetUpDirectionalLights() {
        var light = RenderSettings.sun;

        // Set up the light's color, direction
        // The direction of the light is typically the negated forward
        buffer.SetGlobalVector(DirLightColorID, light.color.linear * light.intensity);
        buffer.SetGlobalVector(DirLightDirectionID, -light.transform.forward);
    }
    */
}
