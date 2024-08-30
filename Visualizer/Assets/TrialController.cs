using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;

public class TrialController : MonoBehaviour
{
    [SerializeField] private ArmController _armController;
    private Queue<IEnumerator> _routineQueue = new Queue<IEnumerator>();

    private void Start()
    {
        Hardpoint hardPoint = new Hardpoint("localhost:6379");
        var buddyParams = hardPoint.GetBuddyParameters("trial_controller");
        string pose1 = buddyParams["pose_name1"];
        string pose2 = buddyParams["pose_name2"];
        string pose3 = buddyParams["pose_name3"];
        float wait_time = TryParseFloat(buddyParams["wait_time"]) ?? 5f; //if can't find, make 2 by default
        float traverse_time = TryParseFloat(buddyParams["traverse_time"]) ?? 5f; //if can't find, make 2 by default

        print("starting trial");
        // Load the initial armature position
        _armController.LoadPose("InitialPose.json");
        string basePath = Path.Combine(Application.streamingAssetsPath, "Track1");
        string pose1Path = Path.Combine(basePath, pose1);
        string pose2Path = Path.Combine(basePath, pose2);
        string pose3Path = Path.Combine(basePath, pose3);
        var armature1 = new ArmaturePosition(pose1Path);
        var armature2 = new ArmaturePosition(pose2Path);
        var armature3 = new ArmaturePosition(pose3Path);

        // Add the trial routines to the queue
        _routineQueue.Enqueue(Wait(wait_time));
        _routineQueue.Enqueue(LoadAndWait(0f, armature1));
        _routineQueue.Enqueue(Wait(wait_time));
        _routineQueue.Enqueue(TraverseToTarget(armature1, armature2, traverse_time));
        _routineQueue.Enqueue(Wait(wait_time));
        _routineQueue.Enqueue(TraverseToTarget(armature2, armature3, traverse_time));
        StartCoroutine(StartTask());

    }

    private float? TryParseFloat(string data)
    {
        float parsedValue;
        bool canParse = float.TryParse(data, out parsedValue);
        return canParse ? parsedValue : null;
    }

    private IEnumerator StartTask()
    {
        while (_routineQueue.Count > 0)
        {
            yield return StartCoroutine(_routineQueue.Dequeue());
        }
    }

    private IEnumerator Wait(float delay)
    {
        print("waiting");
        yield return new WaitForSeconds(delay);
    }

    private IEnumerator LoadAndWait(float delay, ArmaturePosition armature)
    {
        print("loading and waiting");
        _armController.AdjustAngles(armature.GetPositions());
        yield return new WaitForSeconds(delay);
    }

    private IEnumerator TraverseToTarget(ArmaturePosition start, ArmaturePosition target, float duration)
    {
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            ArmaturePosition currentPosition = ArmaturePosition.Lerp(start, target, t);
            _armController.AdjustAngles(currentPosition.GetPositions());
            elapsedTime = Time.time - startTime;
            yield return null;
        }
        _armController.AdjustAngles(target.GetPositions());
    }
}

public class ArmaturePosition
{
    private Dictionary<DOF, float> _dofPositions;

    public ArmaturePosition(Dictionary<DOF, float> dofPositions)
    {
        _dofPositions = dofPositions;
    }

    public ArmaturePosition(string fileName)
    {
        LoadArmatureFromFile(fileName);
    }

    //loads a pose from a JSON file of a DOF dict 
    public void LoadArmatureFromFile(string fileName)
    {
        Debug.Log($"loading {fileName}");
        try
        {
            string jsonString = File.ReadAllText(fileName);
            _dofPositions = JsonConvert.DeserializeObject<Dictionary<DOF, float>>(jsonString);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading pose: {ex.Message}");
        }
    }

    public static ArmaturePosition Lerp(ArmaturePosition a, ArmaturePosition b, float t)
    {
        return a + (b - a) * t;
    }

    public Dictionary<DOF, float> GetPositions()
    {
        return _dofPositions;
    }

    // Overload the + operator
    public static ArmaturePosition operator +(ArmaturePosition a, ArmaturePosition b)
    {
        // Create a new dictionary to store the result of the addition
        var combinedPositions = new Dictionary<DOF, float>(a._dofPositions);

        // Iterate through each key-value pair in the second ArmaturePosition
        foreach (var kvp in b._dofPositions)
        {
            // If the key is already present, add the values together
            if (combinedPositions.ContainsKey(kvp.Key))
            {
                combinedPositions[kvp.Key] += kvp.Value;
            }
            else
            {
                // If the key is not present in the first ArmaturePosition, add it to the result
                combinedPositions.Add(kvp.Key, kvp.Value);
            }
        }

        // Return a new ArmaturePosition instance with the combined positions
        return new ArmaturePosition(combinedPositions);
    }

    // Override the - operator
    public static ArmaturePosition operator -(ArmaturePosition a, ArmaturePosition b)
    {
        // Create a new dictionary to store the result of the subtraction
        var resultPositions = new Dictionary<DOF, float>(a._dofPositions);

        // Iterate through each key-value pair in the second ArmaturePosition
        foreach (var kvp in b._dofPositions)
        {
            // If the key is already present, subtract the value from the first ArmaturePosition
            if (resultPositions.ContainsKey(kvp.Key))
            {
                resultPositions[kvp.Key] -= kvp.Value;
            }
            else
            {
                // If the key is not present in the first ArmaturePosition, add it with a negative value
                resultPositions.Add(kvp.Key, -kvp.Value);
            }
        }

        // Return a new ArmaturePosition instance with the result positions
        return new ArmaturePosition(resultPositions);
    }

    // Override the * operator to scale the positions by a multiplier
    public static ArmaturePosition operator *(ArmaturePosition a, float multiplier)
    {
        // Create a new dictionary to store the scaled positions
        var scaledPositions = new Dictionary<DOF, float>();

        // Iterate through each key-value pair in the ArmaturePosition
        foreach (var kvp in a._dofPositions)
        {
            // Scale the value by the multiplier
            scaledPositions[kvp.Key] = kvp.Value * multiplier;
        }

        // Return a new ArmaturePosition instance with the scaled positions
        return new ArmaturePosition(scaledPositions);
    }
}