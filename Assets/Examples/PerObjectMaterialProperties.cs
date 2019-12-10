using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour {

    static int BaseColorID = Shader.PropertyToID("_BaseColor");
    static MaterialPropertyBlock Block;

    public Color BaseColor = Color.white;

    void Awake() {
        OnValidate();
    }

    void OnValidate() {
        if (Block == null) {
            Block = new MaterialPropertyBlock {};
        }

        Block.SetColor(BaseColorID, BaseColor);
        GetComponent<Renderer>().SetPropertyBlock(Block);
    }
}
