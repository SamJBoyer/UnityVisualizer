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
        Hardpoint hardPoint = new Hardpoint();
        var buddyParams = hardPoint.GetBuddyParameters("trial_controller");
        string pose1 = buddyParams["pose_name1"];
        string pose2 = buddyParams["pose_name2"];
        string pose3 = buddyParams["pose_name3"];
        float wait_time = TryParseFloat(buddyParams["wait_time"]) ?? 5f; //if can't find, make 2 by default
        float traverse_time = TryParseFloat(buddyParams["traverse_time"]) ?? 5f; //if can't find, make 2 by default

        print("starting trial");
        // Load the initial armature position
        //_armController.LoadPose("InitialPose.json");
        string basePath = Path.Combine(Application.streamingAssetsPath, "Track1");
        string pose1Path = Path.Combine(basePath, pose1);
        string pose2Path = Path.Combine(basePath, pose2);
        string pose3Path = Path.Combine(basePath, pose3);
        var armature1 = new ArmatureStructure(pose1Path);
        var armature2 = new ArmatureStructure(pose2Path);
        var armature3 = new ArmatureStructure(pose3Path);

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

    private IEnumerator LoadAndWait(float delay, ArmatureStructure armature)
    {
        print("loading and waiting");
        _armController.AdjustAngles(armature.GetValues());
        yield return new WaitForSeconds(delay);
    }

    private IEnumerator TraverseToTarget(ArmatureStructure start, ArmatureStructure target, float duration)
    {
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            ArmatureStructure currentPosition = ArmatureStructure.Lerp(start, target, t);
            _armController.AdjustAngles(currentPosition.GetValues());
            elapsedTime = Time.time - startTime;
            yield return null;
        }
        _armController.AdjustAngles(target.GetValues());
    }
}

