using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(XmlLoad))]
public class XmlLoad_editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        XmlLoad myScript = (XmlLoad)target;
        if (GUILayout.Button("Load XML"))
        {
            myScript.LoadXml();
        }
    }
}