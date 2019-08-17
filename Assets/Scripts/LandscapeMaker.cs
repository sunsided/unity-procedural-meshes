using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LandscapeMaker : MonoBehaviour
{
    [Header("Area")]
    [SerializeField]
    private float cellSize = 1f;

    [SerializeField]
    private int width = 50;
    [SerializeField]
    private int height = 50;

    [Header("Landscape")]
    [SerializeField]
    private float bumpiness = 5f;

    [SerializeField]
    private float bumpHeight = 5f;

    [Header("Animation")]
    [Range(-10f, 10f)]
    [SerializeField]
    private float animationSpeedX = 1.0f;

    [Range(-10f, 10f)]
    [SerializeField]
    private float animationSpeedY = 1.0f;

    private MeshFilter _meshFilter;
    private MeshRenderer _renderer;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        var mb = new MeshBuilder(6);
        var meshWidth = width + 1;
        var meshHeight = height + 1;

        // Generate the points for the plane.
        var points = new Vector3[meshWidth, meshHeight];
        for (var x = 0; x < meshWidth; ++x)
        {
            for (var y = 0; y < meshHeight; ++y)
            {
                var cellHeight = CalculateHeight(x, y);
                points[x, y] = new Vector3(
                    cellSize * x,
                    cellHeight,
                    cellSize * y);
            }
        }

        var materialCount = _renderer.materials.Length;

        for (var x = 0; x < meshWidth - 1; ++x)
        {
            for (var y = 0; y < meshHeight - 1; ++y)
            {
                var br = points[x, y];
                var bl = points[x + 1, y];
                var tr = points[x, y + 1];
                var tl = points[x + 1, y + 1];

                // Get the average height
                var average = (tl + tr + bl + br) * .25f;
                var faceHeight = average.y;

                var heightFraction = Mathf.Clamp(faceHeight / bumpHeight, 0f, 1f);
                var scaledHeight = Mathf.FloorToInt(heightFraction * (materialCount + 1));
                var submesh = Mathf.Clamp(scaledHeight, 0, materialCount - 1);

                mb.BuildTriangle(bl, tr, tl, submesh);
                mb.BuildTriangle(bl, br, tr, submesh);
            }
        }

        _meshFilter.mesh = mb.CreateMesh();
    }

    private float CalculateHeight(int x, int y)
    {
        var position = transform.position;

        // We can independently animate the noise along the X and Y axes.
        var perlinAnimX = Time.time * animationSpeedX;
        var perlinAnimY = Time.time * animationSpeedY;

        // By offsetting the noise coordinates with the position we're able
        // to tile the landscape, resulting in a reduced triangle count per instance.
        var perlinX = (x + perlinAnimX + position.x) * bumpiness * 0.1f;
        var perlinY = (y + perlinAnimY + position.z) * bumpiness * 0.1f;

        return Mathf.PerlinNoise(perlinX, perlinY) * bumpHeight;
    }
}