﻿using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/CustomRenderPipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset {

    protected override RenderPipeline CreatePipeline() {
        throw new System.NotImplementedException();
    }
}