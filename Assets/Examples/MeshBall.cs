using UnityEngine;

public class MeshBall : MonoBehaviour {

    static MaterialPropertyBlock Block;
    static int BaseColorID = Shader.PropertyToID("_BaseColor");

    public Mesh Mesh = default;
    public Material Material = default;

    Matrix4x4[] matrices = new Matrix4x4[1023];
    Vector4[] baseColors = new Vector4[1023];

    void Awake() {
        for (int i = 0; i < matrices.Length; i++) {
            matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10.0f, Quaternion.Euler(Random.value * 360f,
                Random.value * 360f, Random.value * 360f), Vector3.one * Random.Range(0.5f, 1f));
            baseColors[i] = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1f));
        }
    }

    void Update() {
        if (Block == null) {
            Block = new MaterialPropertyBlock {};
            Block.SetVectorArray(BaseColorID, baseColors);
        }

        Graphics.DrawMeshInstanced(Mesh, 0, Material, matrices, 1023, Block);
    }
}
