using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChessBoardMaker : MonoBehaviour
{
    public float cellSize = 1f;
    public int widthCells = 8;
    public int heightCells = 8;

    private MeshFilter _meshFilter;
    private MeshRenderer _renderer;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        // Skip a zero-sized board.
        var hasCells = widthCells > 0 && heightCells > 0;
        _renderer.enabled = hasCells;
        if (!hasCells)
        {
            return;
        }

        // Render the board.
        var mb = new MeshBuilder(2);
        CreateChessBoard(mb);
        _meshFilter.mesh = mb.CreateMesh();
    }

    private void CreateChessBoard([NotNull] MeshBuilder mb)
    {
        var halfCellSize = cellSize * 0.5f;

        // In order to render one cell, we need two points in each direction.
        // For example, for eight cells, nine vertices need to be created along each axis,
        // such that each pair of vertices creates one of the eight segments.
        var width = widthCells + 1;
        var height = heightCells + 1;
        var halfWidth = Mathf.CeilToInt(width * 0.5f);
        var halfHeight = Mathf.CeilToInt(height * 0.5f);

        // If the number of cells is odd, we need to adjust for an extra half of a cell.
        // We're making use of the fact here that width is an integer: The binary and
        // with 1 isolates the last bit, thus testing whether the value is even (if 0), or odd (if 1).
        // Since it the result of the operation is an integer, we can use it to multiply it in,
        // creating a branch-less version of a check for a half or full (i.e., 2x half) offset.
        var widthOffset = halfCellSize + (width & 1) * halfCellSize;
        var heightOffset = halfCellSize + (height & 1) * halfCellSize;

        // Generate the points for the plane.
        var points = new Vector3[width, height];
        for(var x = -halfWidth; x < -halfWidth + width; ++x)
        {
            for (var y = -halfHeight; y < -halfHeight + height; ++y)
            {
                points[x + halfWidth, y + halfHeight] = new Vector3(
                    (cellSize * x) + widthOffset,
                    0,
                    (cellSize * y) + heightOffset);
            }
        }

        // Generate the faces.
        for(var x = 0; x < width - 1; ++x)
        {
            for (var y = 0; y < height - 1; ++y)
            {
                var br = points[x, y];
                var bl = points[x + 1, y];
                var tr = points[x, y + 1];
                var tl = points[x + 1, y + 1];

                var submesh = (x + y) & 1;
                mb.BuildTriangle(bl, tr, tl, submesh);
                mb.BuildTriangle(bl, br, tr, submesh);
            }
        }
    }
}