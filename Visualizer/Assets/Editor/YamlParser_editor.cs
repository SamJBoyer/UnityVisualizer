using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(YamlParser))]
public class YamlParser_editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        YamlParser myScript = (YamlParser)target;
        if (GUILayout.Button("Load Yaml"))
        {
            myScript.LoadYaml();
        }
        if (GUILayout.Button("Write Yaml"))
        {
            myScript.WriteYamlToFile();
        }
    }
}