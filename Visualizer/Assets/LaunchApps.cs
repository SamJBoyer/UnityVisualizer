using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LaunchApps : MonoBehaviour
{
    // Path to the Python executable
    public string pythonPath = "/home/sam/anaconda3/envs/rt/bin/python3.8"; // Adjust this path as necessary

    // Path to the Python script
    public string scriptPath = "/home/sam/Desktop/runtest.py"; // Adjust this path as necessary

    private void Start()
    {
        LaunchScript();
    }

    // Method to launch the Python script
    public void LaunchScript()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = pythonPath;
        startInfo.Arguments = scriptPath;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();

        // Read the output (optional)
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        // Log the output and error (optional)
        Debug.Log("Output: " + output);
        Debug.Log("Error: " + error);
    }
}