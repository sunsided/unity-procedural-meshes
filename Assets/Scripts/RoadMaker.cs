using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class RoadMaker : MonoBehaviour
{
    public float radius = 30f;
    public int segments = 300;

    public GameObject car;

    public float lineWidth = 0.3f;
    public float roadWidth = 8f;
    public float edgeWidth = 1f;
    public float edgeHeight = 1f;

    public float waviness = 5f;
    public float waveScale = .1f;

    public Vector2 waveOffset;
    public Vector2 waveStep = new Vector2(0.01f, 0.01f);

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    private void Update()
    {
        var mb = new MeshBuilder(6);

        var numberWaypoints = segments;
        var segmentDegrees = 360f / numberWaypoints;
        var points = new List<Vector3>();
        for (var degrees = 0f; degrees < 360f; degrees += segmentDegrees)
        {
            var distance = Vector3.forward * radius;
            var point = Quaternion.AngleAxis(degrees, Vector3.up) * distance;
            points.Add(point);
        }

        // Apply noise.
        var wave = waveOffset;
        for (var i = 0; i < points.Count; ++i)
        {
            wave += waveStep;

            var p0 = points[i];
            var centerDir = p0.normalized;
            var sample = Mathf.PerlinNoise(wave.x * waveScale, wave.y * waveScale) * waviness;

            // Somewhat fix the seam between the start and the end of the road.
            var control = Mathf.PingPong(i, points.Count * 0.5f) / (points.Count * 0.5f);

            points[i] += centerDir * (sample * control);
        }

        for (var i = 1; i < points.Count + 1; ++i)
        {
            var pPrev = points[i - 1];
            var p0 = points[i % points.Count];
            var p1 = points[(i + 1) % points.Count];

            ExtrudeRoad(mb, pPrev, p0, p1);
        }

        _meshFilter.mesh = mb.CreateMesh();
        _meshCollider.sharedMesh = _meshFilter.mesh;
    }

    private void ExtrudeRoad(MeshBuilder mb, Vector3 pPrev, Vector3 p0, Vector3 p1)
    {
        // Line
        var offset = Vector3.zero;
        var target = Vector3.forward * lineWidth;
        MakeRoadQuad(mb, pPrev, p0, p1, offset, target, 0);

        // Road
        offset += target;
        target = Vector3.forward * roadWidth;
        MakeRoadQuad(mb, pPrev, p0, p1, offset, target, 1);

        // Edge wall inner side
        offset += target;
        target = Vector3.up * edgeHeight;
        MakeRoadQuad(mb, pPrev, p0, p1, offset, target, 2);

        // Edge wall top
        offset += target;
        target = Vector3.forward * edgeWidth;
        MakeRoadQuad(mb, pPrev, p0, p1, offset, target, 2);

        // Edge wall outer side
        offset += target;
        target = Vector3.down * edgeHeight;
        MakeRoadQuad(mb, pPrev, p0, p1, offset, target, 2);
    }

    private void MakeRoadQuad([NotNull] MeshBuilder mb, Vector3 pPrev, Vector3 p0, Vector3 p1, Vector3 offset,
        Vector3 targetOffset, int submesh)
    {
        var forward = (p1 - p0).normalized;
        var forwardPrev = (p0 - pPrev).normalized;

        // Outer side of the road
        var perpendicular = Quaternion.LookRotation(
            Vector3.Cross(forward, Vector3.up)
        );

        var perpendicularPrev = Quaternion.LookRotation(
            Vector3.Cross(forwardPrev, Vector3.up)
        );

        var tl = p0 + perpendicularPrev * offset;
        var tr = p0 + perpendicularPrev * (offset + targetOffset);

        var bl = p1 + perpendicular * offset;
        var br = p1 + perpendicular * (offset + targetOffset);

        mb.BuildTriangle(tl, tr, bl, submesh);
        mb.BuildTriangle(tr, br, bl, submesh);

        // Inner side of the road
        perpendicular = Quaternion.LookRotation(
            Vector3.Cross(-forward, Vector3.up)
        );

        perpendicularPrev = Quaternion.LookRotation(
            Vector3.Cross(-forwardPrev, Vector3.up)
        );

        tl = p0 + perpendicularPrev * offset;
        tr = p0 + perpendicularPrev * (offset + targetOffset);

        bl = p1 + perpendicular * offset;
        br = p1 + perpendicular * (offset + targetOffset);

        // Note that we needed to flip the rendering order.
        mb.BuildTriangle(bl, br, tl, submesh);
        mb.BuildTriangle(br, tr, tl, submesh);
    }
}