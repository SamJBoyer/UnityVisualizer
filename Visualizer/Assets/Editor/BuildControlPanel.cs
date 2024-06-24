using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class BuildControlPanel
{
    public static void Build()
    {
        
        string[] scenes = { "Assets/Scenes/ControlPanel.unity" }; // Adjust the scene path as needed
        string buildPath = System.Environment.GetCommandLineArgs()[System.Environment.GetCommandLineArgs().Length - 1];

        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneLinux64, BuildOptions.None);
    }
}
