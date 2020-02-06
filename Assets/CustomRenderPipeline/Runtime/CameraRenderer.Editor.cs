using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

partial class CameraRenderer 
{

    partial void PrepareBuffer();
    partial void PrepareForSceneWindow();
    partial void DrawGizmos();
    partial void DrawUnsupportedShaders();

#if UNITY_EDITOR
    static ShaderTagId[] LegacyShaderTagIDs = 
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    static Material ErrorMaterial;

    public string SampleName { get; set; }

    partial void DrawGizmos() 
    {
        if (Handles.ShouldRenderGizmos()) {
            ctx.DrawGizmos(cam, GizmoSubset.PreImageEffects);
            ctx.DrawGizmos(cam, GizmoSubset.PostImageEffects);
        }
    }

    partial void DrawUnsupportedShaders() 
    {
        if (ErrorMaterial == null) 
        {
            ErrorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawingSettings = new DrawingSettings(LegacyShaderTagIDs[0], new SortingSettings(cam)) {
            overrideMaterial = ErrorMaterial
        };
        for (int i = 1; i < LegacyShaderTagIDs.Length; i++) 
        {
            drawingSettings.SetShaderPassName(i, LegacyShaderTagIDs[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        ctx.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    partial void PrepareForSceneWindow() 
    {
        if (cam.cameraType == CameraType.SceneView) 
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(cam);
        }
    }

    partial void PrepareBuffer() 
    {
        Profiler.BeginSample("Editor Only");
        buffer.name = SampleName = cam.name;
        Profiler.EndSample();
    }
#else
    string SampleName => BufferName;
#endif
}
