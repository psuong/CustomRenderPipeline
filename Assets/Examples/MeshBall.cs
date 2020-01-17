using UnityEngine;

public class MeshBall : MonoBehaviour {

    static MaterialPropertyBlock Block;

    static int BaseColorID  = Shader.PropertyToID("_BaseColor"),
               MetallicID   = Shader.PropertyToID("_Metallic"),
               SmoothnessID = Shader.PropertyToID("_Smoothness");

    public Mesh Mesh = default;
    public Material Material = default;

    Matrix4x4[] matrices = new Matrix4x4[1023];
    Vector4[] baseColors = new Vector4[1023];
    float[] metallic     = new float[1023],
            smoothness   = new float[1023];

    void Awake() {
        for (int i = 0; i < matrices.Length; i++) {
            matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10.0f, Quaternion.Euler(Random.value * 360f,
                Random.value * 360f, Random.value * 360f), Vector3.one * Random.Range(0.5f, 1f));
            baseColors[i] = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1f));
            metallic[i] = Random.value < 0.25f ? 1f : 0f;
            smoothness[i] = Random.Range(0.05f, 0.95f);
        }
    }

    void Update() {
        if (Block == null) {
            Block = new MaterialPropertyBlock {};
            Block.SetVectorArray(BaseColorID, baseColors);
            Block.SetFloatArray(MetallicID, metallic);
            Block.SetFloatArray(MetallicID, metallic);
        }

        Graphics.DrawMeshInstanced(Mesh, 0, Material, matrices, 1023, Block);
    }
}
