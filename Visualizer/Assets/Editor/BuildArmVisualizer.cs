using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class BuildArmVisualizer
{
    public static void Build()
    {
        string[] scenes = { "Assets/Scenes/ArmVisualizer.unity" }; // Adjust the scene path as needed
        string buildPath = System.Environment.GetCommandLineArgs()[System.Environment.GetCommandLineArgs().Length - 1]; //i could use this in the future to get argumetents to dictate what scene to put in some there only needed to be 1 buidl script 

        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneLinux64, BuildOptions.None);
    }
}
