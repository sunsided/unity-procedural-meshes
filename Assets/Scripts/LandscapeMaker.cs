using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LandscapeMaker : MonoBehaviour
{
    [Header("Area")]
    public float cellSize = 1f;
    public int width = 24;
    public int height = 24;

    [Header("Landscape")]
    public float bumpiness = 5f;
    public float bumpHeight = 5f;

    [Header("Animation")]
    [Range(-10f, 10f)]
    public float animationSpeedX = 1.0f;
    [Range(-10f, 10f)]
    public float animationSpeedY = 1.0f;

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
        for (var x = 0; x < width; ++x)
        {
            for (var y = 0; y < height; ++y)
            {
                var perlinX = (x + Time.time * animationSpeedX) * bumpiness * 0.1f;
                var perlinY = (y + Time.time * animationSpeedY) * bumpiness * 0.1f;
                var noise = Mathf.PerlinNoise(perlinX, perlinY) * bumpHeight;

                points[x, y] = new Vector3(
                    cellSize * x,
                    noise,
                    cellSize * y);
            }
        }

        var submesh = 0;
        for (var x = 0; x < width - 1; ++x)
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