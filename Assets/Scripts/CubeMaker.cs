using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CubeMaker : MonoBehaviour
{
    public Vector3 size = Vector3.one;

    private MeshFilter _meshFilter;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        // TODO: We'd like to avoid the allocations each time ...
        var mb = new MeshBuilder(6);

        var cubeSize = size * 0.5f;

        // Top square
        var t0 = new Vector3(cubeSize.x, cubeSize.y, -cubeSize.z); // front-left
        var t1 = new Vector3(-cubeSize.x, cubeSize.y, -cubeSize.z); // front-right
        var t2 = new Vector3(-cubeSize.x, cubeSize.y, cubeSize.z); // back-right
        var t3 = new Vector3(cubeSize.x, cubeSize.y, cubeSize.z); // back-left

        // Bottom square
        var b0 = new Vector3(cubeSize.x, -cubeSize.y, -cubeSize.z); // front-left
        var b1 = new Vector3(-cubeSize.x, -cubeSize.y, -cubeSize.z); // front-right
        var b2 = new Vector3(-cubeSize.x, -cubeSize.y, cubeSize.z); // back-right
        var b3 = new Vector3(cubeSize.x, -cubeSize.y, cubeSize.z); // back-left

        // Top face (+Y, up direction)
        mb.BuildTriangle(t0, t1, t2, 0);
        mb.BuildTriangle(t0, t2, t3, 0);

        // Bottom face (-Y, down direction)
        mb.BuildTriangle(b2, b1, b0, 1);
        mb.BuildTriangle(b3, b2, b0, 1);

        // Back face (-Z, backward direction)
        mb.BuildTriangle(b0, t1, t0, 2);
        mb.BuildTriangle(b0, b1, t1, 2);

        // Left face (-X, left direction)
        mb.BuildTriangle(b1, t2, t1, 3);
        mb.BuildTriangle(b1, b2, t2, 3);

        // Front face (+Z, forward direction)
        mb.BuildTriangle(b2, t3, t2, 4);
        mb.BuildTriangle(b2, b3, t3, 4);

        // Right face (+X, right direction)
        mb.BuildTriangle(b3, t0, t3, 5);
        mb.BuildTriangle(b3, b0, t0, 5);

        _meshFilter.mesh = mb.CreateMesh();
    }
}