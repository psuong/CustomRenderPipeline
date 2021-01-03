using System;
using UnityEngine;

public enum TextureSize {
    _256  = 256,
    _512  = 512,
    _1024 = 1024,
    _2048 = 2048,
    _4096 = 4096,
    _8192 = 8192
}

[Serializable]
public class ShadowSettings {

    [Serializable]
    public struct Directional {
        public TextureSize AtlasSize;
    }
    
    [Min(0f)]
    public float MaxDistance = 100f;

    public Directional DirectionalShadows = new Directional { AtlasSize = TextureSize._1024 };
}

