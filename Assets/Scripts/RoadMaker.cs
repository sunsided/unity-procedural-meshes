﻿using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public enum TrackShape
{
    Circle,
    Advanced
}

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class RoadMaker : MonoBehaviour
{
    [Header("Track shape")]
    [SerializeField]
    private TrackShape generator = TrackShape.Circle;

    [Header("Road dimensions")]
    [SerializeField]
    private float radius = 30f;

    [SerializeField]
    private int segments = 300;

    [SerializeField]
    private float lineWidth = 0.3f;

    [SerializeField]
    private float roadWidth = 8f;

    [SerializeField]
    private float edgeWidth = 1f;

    [SerializeField]
    private float edgeHeight = 1f;

    [Header("Shape variation")]
    [SerializeField]
    private float waviness = 5f;

    [SerializeField]
    private float waveScale = .1f;

    [SerializeField]
    private Vector2 waveOffset;

    [SerializeField]
    private Vector2 waveStep = new Vector2(0.01f, 0.01f);

    [Header("Advances track shape")]
    [SerializeField]
    private float stepLength = 1f;

    [SerializeField]
    private float stepMaxAngle = 1f;

    [SerializeField]
    private int shapeSides = 3;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private bool _stripeCheck;

    public bool AutoRebuild { get; set; }

    public void RebuildTrack()
    {
        var points = generator == TrackShape.Circle
            ? MakeCircleBasedTrack()
            : MakeAdvancedTrack(shapeSides);

        var mesh = CreateMesh(points);

        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = _meshFilter.sharedMesh;
    }

    [NotNull]
    private List<Vector3> MakeCircleBasedTrack()
    {
        var numberWaypoints = segments;
        var segmentDegrees = 360f / numberWaypoints;
        var points = new List<Vector3>();
        for (var degrees = 0f; degrees < 360f; degrees += segmentDegrees)
        {
            var distance = Vector3.forward * radius;
            var point = Quaternion.AngleAxis(degrees, Vector3.up) * distance;
            points.Add(point);
        }

        ApplyVariation(points);

        return points;
    }

    /// <summary>
    ///     Builds a more realistic track.
    /// </summary>
    /// <remarks>
    ///     Note that this code is taken straight from Oliver Carson and only updated a bit for style.
    ///     It does not function entirely correct in that it generates self-crossing tracks,
    ///     but the results are looking amazing in comparison to the circle-based approach.
    /// </remarks>
    [NotNull]
    private List<Vector3> MakeAdvancedTrack(int sides)
    {
        // Create a point list of 4 corners;
        var points = new List<Vector3>(sides + 1);

        // Create shape corners.
        for (var i = 0; i < sides; i++)
        {
            var forwardVector = Vector3.forward * radius;
            var point = Quaternion.AngleAxis(90 * i, Vector3.up) * forwardVector;
            points.Add(point);
        }

        // Add a random point of variance.
        var vector = Vector3.forward * (radius * 0.5f);
        var randomPoint = Quaternion.AngleAxis(90f * Random.Range(0, 10) + 45f, Vector3.up) * vector;
        points.Insert(Random.Range(0, points.Count), randomPoint);

        ApplyVariation(points);

        // Create a traverser to create multiple points along the line, and to widen turns.
        var dividePoints = new List<Vector3>();

        var dir = (points[points.Count - 1] - points[points.Count - 2]).normalized;
        var traversePoint = points[0];

        // Get a corner and the target corner, and keep doing that till all sides built,
        // do this several times to smooth out the shape.
        for (var j = 0; j < 6; ++j)
        {
            dividePoints.Clear();

            for (var i = 0; i < points.Count; ++i)
            {
                var c0 = points[i];
                var c1 = points[(i + 1) % points.Count];
                float traverseDist = 0;

                while (traverseDist < Vector3.Distance(c0, c1))
                {
                    traverseDist += stepLength;
                    traversePoint += dir * stepLength;
                    dividePoints.Add(traversePoint);
                    dir = Quaternion.RotateTowards(
                              Quaternion.LookRotation(dir),
                              Quaternion.LookRotation((c1 - traversePoint).normalized),
                              stepMaxAngle) * Vector3.forward;
                }
            }
        }

        return dividePoints;
    }

    private void ApplyVariation<T>([NotNull] T points)
        where T : IList<Vector3>
    {
        var wave = waveOffset;
        for (var i = 0; i < points.Count; ++i)
        {
            wave += waveStep;

            var p0 = points[i];
            var centerDir = p0.normalized;
            var sample = Mathf.PerlinNoise(wave.x * waveScale + waveOffset.x, wave.y * waveScale + waveOffset.y) *
                         waviness;

            // Somewhat fix the seam between the start and the end of the road.
            var control = Mathf.PingPong(i, points.Count * 0.5f) / (points.Count * 0.5f);

            points[i] += centerDir * (sample * control);
        }
    }

    private Mesh CreateMesh<T>([NotNull] T points)
        where T : IList<Vector3>
    {
        var mb = new MeshBuilder(6);
        for (var i = 1; i < points.Count + 1; ++i)
        {
            var pPrev = points[i - 1];
            var p0 = points[i % points.Count];
            var p1 = points[(i + 1) % points.Count];

            ExtrudeRoad(mb, pPrev, p0, p1);
        }

        var mesh = mb.CreateMesh();
        return mesh;
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

        var stripeSubmesh = 2;
        if (_stripeCheck)
        {
            stripeSubmesh = 3;
        }

        _stripeCheck = !_stripeCheck;

        // Edge wall inner side
        offset += target;
        target = Vector3.up * edgeHeight;
        MakeRoadQuad(mb, pPrev, p0, p1, offset, target, stripeSubmesh);

        // Edge wall top
        offset += target;
        target = Vector3.forward * edgeWidth;
        MakeRoadQuad(mb, pPrev, p0, p1, offset, target, stripeSubmesh);

        // Edge wall outer side
        offset += target;
        target = Vector3.down * edgeHeight;
        MakeRoadQuad(mb, pPrev, p0, p1, offset, target, stripeSubmesh);
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

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    private void Start()
    {
        RebuildTrack();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (AutoRebuild && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            RebuildTrack();
        }
    }
#endif
}