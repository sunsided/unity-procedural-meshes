using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadMaker))]
public class RoadMakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var roadMaker = (RoadMaker)target;

        EditorGUILayout.LabelField("Mesh rebuilding", EditorStyles.boldLabel);

        roadMaker.AutoRebuild = EditorGUILayout.Toggle("Rebuild automatically", roadMaker.AutoRebuild);
        if (roadMaker.AutoRebuild)
        {
            EditorGUILayout.HelpBox(
                "The road mesh is going to be rebuilt automatically.\nThis may slow down your editor experience.",
                MessageType.Warning);
        }
        else
        {
            if (GUILayout.Button("Rebuild"))
            {
                roadMaker.RebuildTrack();
            }
        }

        EditorGUILayout.Separator();

        DrawDefaultInspector();
    }
}