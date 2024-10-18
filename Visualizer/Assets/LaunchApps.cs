using System.Diagnostics;
using UnityEngine;

public class LaunchApps : MonoBehaviour
{
    public string scriptPath = "/home/sam/projects/brand/setup.sh"; // Update with your script path

    public void LaunchScript()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash", // or "/bin/sh"
            Arguments = $"-c \"{scriptPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true // Set to false if you want to see the window
        };

        using (Process process = new Process())
        {
            process.StartInfo = startInfo;

            try
            {
                process.Start();

                // Optionally read the output and error streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                UnityEngine.Debug.Log("Output: " + output);
                UnityEngine.Debug.Log("Errors: " + error);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError("An error occurred: " + ex.Message);
            }
        }
    }

    // Example usage: Call this method on a button press
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Change to your preferred input method
        {
            LaunchScript();
        }
    }
}
