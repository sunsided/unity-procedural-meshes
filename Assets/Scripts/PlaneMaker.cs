using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PlaneMaker : MonoBehaviour
{
    public float cellSize = 1f;
    public int width = 24;
    public int height = 24;

    private MeshFilter _meshFilter;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        var mb = new MeshBuilder(6);

        // Generate the points for the plane.
        var points = new Vector3[width, height];
        for(var x = 0; x < width; ++x)
        {
            for (var y = 0; y < height; ++y)
            {
                points[x, y] = new Vector3(
                    cellSize * x,
                    Mathf.PingPong(x, 10),
                    cellSize * y);
            }
        }

        var submesh = 0;
        for(var x = 0; x < width - 1; ++x)
        {
            for (var y = 0; y < height - 1; ++y)
            {
                var br = points[x, y];
                var bl = points[x + 1, y];
                var tr = points[x, y + 1];
                var tl = points[x + 1, y + 1];

                mb.BuildTriangle(bl, tr, tl, submesh % 6);
                mb.BuildTriangle(bl, br, tr, submesh % 6);
            }

            ++submesh;
        }

        _meshFilter.mesh = mb.CreateMesh();
    }
}
