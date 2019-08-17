using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PyramidMaker : MonoBehaviour
{
    public float pyramidSize = 5f;

    private MeshFilter _meshFilter;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        var mb = new MeshBuilder(4);

        // Top point.
        var top = new Vector3(0, pyramidSize, 0);

        // ReSharper disable once Unity.InefficientMultiplicationOrder
        var distance = Vector3.forward * pyramidSize;

        // Base.
        var base0 = Quaternion.AngleAxis(0f, Vector3.up) * distance;
        var base1 = Quaternion.AngleAxis(240f, Vector3.up) * distance;
        var base2 = Quaternion.AngleAxis(120f, Vector3.up) * distance;

        mb.BuildTriangle(base0, base1, base2, 0);
        mb.BuildTriangle(base1, base0, top, 1);
        mb.BuildTriangle(base2, top, base0, 2);
        mb.BuildTriangle(top, base2, base1, 3);

        _meshFilter.mesh = mb.CreateMesh();
    }
}