using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System;
using static ArmatureStructure;

public class OpenLoopReader : MonoBehaviour
{

    [SerializeField] private ArmController _currentArm;
    [SerializeField] private ArmController _targetArm;

    private Hardpoint _hardpoint;

    void Start()
    {
        _hardpoint = new Hardpoint(new string[] { "current_armature", "target_armature" }, "localhost:6379");
    }


    // Update is called once per frame
    void Update()
    {
        var currentData = _hardpoint.GetInstantData("current_armature");
        var targetData = _hardpoint.DequeueData("target_armature");



        if (targetData != null)
        {
            print("new data");
            var targetDict = currentData.ToDictionary(x => (DOF)Enum.Parse(typeof(DOF), x.Key), x => float.Parse(x.Value));
            _targetArm.AdjustAngles(targetDict);
        }


        var adjustmentDict = currentData.ToDictionary(x => (DOF)Enum.Parse(typeof(DOF), x.Key), x => float.Parse(x.Value));
        _currentArm.AdjustAngles(adjustmentDict);
    }
}
