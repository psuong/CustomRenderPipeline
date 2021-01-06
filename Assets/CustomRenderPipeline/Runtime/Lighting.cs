using UnityEngine;
using UnityEngine.Rendering;

public class Lighting {

    const string BufferName    = "Lighting";
    const int MaxDirLightCount = 4;

    static readonly int DirLightCountID      = Shader.PropertyToID("_DirectionalLightCount");
    static readonly int DirLightColorsID     = Shader.PropertyToID("_DirectionalLightColors");
    static readonly int DirLightDirectionsID = Shader.PropertyToID("_DirectionalLightDirections");
    static readonly int DirLightShadowDataID = Shader.PropertyToID("_DirectionalLightShadowData");

    static Vector4[] DirLightColors     = new Vector4[MaxDirLightCount];
    static Vector4[] DirLightDirections = new Vector4[MaxDirLightCount];
    static Vector4[] DirLightShadowData = new Vector4[MaxDirLightCount];

    CullingResults cullingResults;

    Shadows shadows = new Shadows();
    CommandBuffer buffer = new CommandBuffer { name = BufferName };

    public void SetUp(ScriptableRenderContext ctx, CullingResults cullingResults, ShadowSettings shadowSettings) {
        this.cullingResults = cullingResults;

        buffer.BeginSample(BufferName);

        // SetUpDirectionalLights() is obsolete since it only supports 1 light
        // SetUpDirectionalLights();

        // Lighting sets up shadows
        shadows.SetUp(ctx, cullingResults, shadowSettings);

        // Set up all visible lights we want to support - for now only Directional ones :)
        SetUpLights();

        // Render the shadows
        shadows.Render();

        buffer.EndSample(BufferName);
        ctx.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public void CleanUp() {
        shadows.CleanUp();
    }

    void SetUpLights() {
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
        buffer.SetGlobalVectorArray(DirLightShadowDataID, DirLightShadowData);
    }

    void SetUpDirectionLight(int index, ref VisibleLight visibleLight) {
        DirLightColors[index]     = visibleLight.finalColor;
        DirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        DirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, index);
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

