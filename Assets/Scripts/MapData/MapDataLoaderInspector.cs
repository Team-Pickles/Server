using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapDataLoad))]
public class MapDataLoaderInspector : Editor
{
    public MapDataLoad current {
        get {
            return (MapDataLoad)target;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Refresh"))
            current.Refresh();
        if(GUILayout.Button("LoadMap"))
            current.Load();
    }
}
