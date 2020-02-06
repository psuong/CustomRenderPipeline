using UnityEngine;

[System.Serializable]
public class ShadowSettings 
{
    public enum TextureSize
    {
        _256  = 256,
        _512  = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }

    [System.Serializable]
    public struct Directional 
    {
        public TextureSize AtlasSize;
    }

    [Min(0f)]
    public float MaxDistance = 100f;

    public Directional _Directional = new Directional 
    {
        AtlasSize = TextureSize._1024
    };
}
