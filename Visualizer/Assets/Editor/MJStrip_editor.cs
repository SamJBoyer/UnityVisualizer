using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MJStrip))]
public class MJStrip_editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MJStrip myScript = (MJStrip)target;
        if (GUILayout.Button("Remove MJ Components"))
        {
            myScript.RemoveMJComponents();
        }
    }
}