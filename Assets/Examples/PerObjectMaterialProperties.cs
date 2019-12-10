using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour {

    static int BaseColorID = Shader.PropertyToID("_BaseColor");
    static int CutoffID = Shader.PropertyToID("_Cutoff");

    static MaterialPropertyBlock Block;

    public Color BaseColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    public float Cutoff = 0.5f;

    void Awake() {
        OnValidate();
    }

    void OnValidate() {
        if (Block == null) {
            Block = new MaterialPropertyBlock {};
        }

        Block.SetColor(BaseColorID, BaseColor);
        Block.SetFloat(CutoffID, Cutoff);
        GetComponent<Renderer>().SetPropertyBlock(Block);
    }
}
