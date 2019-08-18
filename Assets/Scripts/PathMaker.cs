using System;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class PathShape
{
    [NotNull]
    public Vector3[] shape = { -Vector3.up, Vector3.up, -Vector3.up };
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PathMaker : MonoBehaviour
{
    [SerializeField]
    private Transform[] path;

    [SerializeField]
    private PathShape pathShape;

    private MeshFilter _meshFilter;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        var mb = new MeshBuilder(6);

        var prevShape = TranslateShape(
            path[path.Length - 1].transform.position,
            (path[0].transform.position - path[path.Length - 1].transform.position).normalized,
            pathShape.shape
        );

        for (var i = 0; i < path.Length; ++i)
        {
            var nextShape = TranslateShape(
                path[i].transform.position,
                (path[(i + 1) % path.Length].transform.position - path[i].transform.position).normalized,
                pathShape.shape
            );

            for (var j = 0; j < nextShape.Length - 1; ++j)
            {
                mb.BuildTriangle(prevShape[j], nextShape[j], nextShape[j + 1], 0);
                mb.BuildTriangle(prevShape[j + 1], prevShape[j], nextShape[j + 1], 0);
            }

            prevShape = nextShape;
        }

        _meshFilter.mesh = mb.CreateMesh();
    }

    [NotNull]
    private static Vector3[] TranslateShape(Vector3 position, Vector3 forward, [NotNull] Vector3[] shape)
    {
        var translatedShape = new Vector3[shape.Length];
        var forwardRotation = Quaternion.LookRotation(forward);

        for (var i = 0; i < shape.Length; ++i)
        {
            translatedShape[i] = forwardRotation * shape[i] + position;
        }

        return translatedShape;
    }
}