using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour {

    readonly static int 
        BaseColorID  = Shader.PropertyToID("_BaseColor"),
        CutoffID     = Shader.PropertyToID("_Cutoff"),
        MetallicID   = Shader.PropertyToID("_Metallic"),
        SmoothnessID = Shader.PropertyToID("_Smoothness");

    static MaterialPropertyBlock Block;

    public Color BaseColor = Color.white;
    public float AlphaCutOff = 0.5f, Metallic = 0f, Smoothness = 0.5f;

    [SerializeField, Range(0f, 1f)]
    public float Cutoff = 0.5f;

    void Awake() {
        OnValidate();
    }

    void OnValidate() {
        if (Block == null) {
            Block = new MaterialPropertyBlock { };
        }

        Block.SetColor(BaseColorID, BaseColor);
        Block.SetFloat(CutoffID, Cutoff);
        Block.SetFloat(MetallicID, Metallic);
        Block.SetFloat(SmoothnessID, Smoothness);
        GetComponent<Renderer>().SetPropertyBlock(Block);
    }
}
