using System;
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

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        if (widthCells <= 0 || heightCells <= 0)
        {
            return;
        }

        var mb = new MeshBuilder(6);
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
        var widthOffset = (width & 1) == 0 ? halfCellSize : cellSize;
        var heightOffset = (height & 1) == 0 ? halfCellSize : cellSize;

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
                var submesh = (x + y) & 1;

                var br = points[x, y];
                var bl = points[x + 1, y];
                var tr = points[x, y + 1];
                var tl = points[x + 1, y + 1];

                mb.BuildTriangle(bl, tr, tl, submesh);
                mb.BuildTriangle(bl, br, tr, submesh);
            }
        }
    }
}