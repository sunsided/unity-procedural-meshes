﻿using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LandscapeMaker : MonoBehaviour
{
    [Header("Area")]
    public float cellSize = 1f;
    public int width = 50;
    public int height = 50;

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

        var submesh = 0;
        for (var x = 0; x < meshWidth - 1; ++x)
        {
            for (var y = 0; y < meshHeight - 1; ++y)
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