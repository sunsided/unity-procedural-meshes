using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
///     Mesh builder class as described in Udemy Course
///     <a href="https://www.udemy.com/course/unity_procgen">3D Procedural Mesh Generation Fundamentals in Unity</a>.
/// </summary>
public class MeshBuilder
{
    [NotNull]
    private static readonly int[] DefaultTriangleIndices = { 0, 0, 0 };

    [NotNull]
    private readonly List<int> _indices = new List<int>();

    [NotNull]
    private readonly List<Vector3> _normals = new List<Vector3>();

    [NotNull]
    private readonly List<int>[] _submeshIndices;

    [NotNull]
    private readonly List<Vector2> _uvs = new List<Vector2>();

    [NotNull]
    private readonly List<Vector3> _vertices = new List<Vector3>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MeshBuilder" /> class.
    /// </summary>
    /// <param name="submeshCount">The number of sub-meshes used.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The specified value for <paramref name="submeshCount" /> was
    ///     non-positive.
    /// </exception>
    public MeshBuilder(int submeshCount)
    {
        if (submeshCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(submeshCount), submeshCount,
                "There must be at least one submesh.");
        }

        _submeshIndices = new List<int>[submeshCount];
        for (var i = 0; i < submeshCount; ++i)
        {
            _submeshIndices[i] = new List<int>();
        }
    }

    /// <summary>
    ///     Add a triangle with default normal.
    /// </summary>
    /// <remarks>
    ///     Make sure that the points are in the correct winding order.
    /// </remarks>
    /// <param name="p0">First point of the triangle.</param>
    /// <param name="p1">Second point of the triangle.</param>
    /// <param name="p2">Third point of the triangle.</param>
    /// <param name="submesh">The sub-mesh ID.</param>
    public void BuildTriangle(Vector3 p0, Vector3 p1, Vector3 p2, int submesh)
    {
        var normal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
        BuildTriangle(p0, p1, p2, normal, submesh);
    }

    /// <summary>
    ///     Add a triangle with a defined normal.
    /// </summary>
    /// <remarks>
    ///     Make sure that the points are in the correct winding order.
    /// </remarks>
    /// <param name="p0">First point of the triangle.</param>
    /// <param name="p1">Second point of the triangle.</param>
    /// <param name="p2">Third point of the triangle.</param>
    /// <param name="normal">The normal vector of the triangle face.</param>
    /// <param name="submesh">The sub-mesh ID.</param>
    public void BuildTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 normal, int submesh)
    {
        Func<int, int, int> lol = (a, b) => a + b;

        // The indices of the vertices we are going to add.
        // Ignoring memory considerations (e.g. possible deduplication), we're going to add the vertices
        // one by one to the vertices list. Thus, the index of the first vertex to be
        // added is one plus the index of the last index that was already added, or simply the length
        // of the list.
        var p0Index = _vertices.Count;
        var p1Index = _vertices.Count + 1;
        var p2Index = _vertices.Count + 2;

        _indices.Add(p0Index);
        _indices.Add(p1Index);
        _indices.Add(p2Index);

        Debug.Assert(_submeshIndices.Length > submesh, "_submeshIndices.Length > submesh");
        _submeshIndices[submesh].Add(p0Index);
        _submeshIndices[submesh].Add(p1Index);
        _submeshIndices[submesh].Add(p2Index);

        // We're now adding the vertices and their normals as discussed above.
        _vertices.Add(p0);
        _vertices.Add(p1);
        _vertices.Add(p2);

        _normals.Add(normal);
        _normals.Add(normal);
        _normals.Add(normal);

        // We're using some default UV coordinates here for the time being.
        _uvs.Add(new Vector2(0, 0));
        _uvs.Add(new Vector2(0, 1));
        _uvs.Add(new Vector2(1, 1));
    }

    /// <summary>
    ///     Bakes the registered triangles into a mesh.
    /// </summary>
    /// <returns>The <see cref="Mesh" /> built from the triangles.</returns>
    [NotNull]
    public Mesh CreateMesh()
    {
        var mesh = new Mesh
        {
            vertices = _vertices.ToArray(),
            triangles = _indices.ToArray(),
            normals = _normals.ToArray(),
            uv = _uvs.ToArray(),
            subMeshCount = _submeshIndices.Length
        };

        for (var i = 0; i < _submeshIndices.Length; ++i)
        {
            var submeshIndices = _submeshIndices[i];
            var triangles = SubmeshTriangleIndicesValid(submeshIndices)
                ? submeshIndices.ToArray()
                : DefaultTriangleIndices;
            mesh.SetTriangles(triangles, i);
        }

        return mesh;
    }

    /// <summary>
    ///     Verifies whether a submesh has enough indices for a valid triangle.
    /// </summary>
    /// <param name="submeshIndices">The triangle.</param>
    /// <typeparam name="T">The type of the triangle.</typeparam>
    /// <returns><see langword="true" /> if the triangle is valid; <see langword="false" /> otherwise.</returns>
    private static bool SubmeshTriangleIndicesValid<T>(T submeshIndices)
        where T : ICollection<int>
        => submeshIndices.Count >= 3;
}