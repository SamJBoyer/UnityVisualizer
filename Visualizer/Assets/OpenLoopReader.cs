using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System;

public class OpenLoopReader : MonoBehaviour
{

    [SerializeField] private ArmController _armController;
    private string _streamName = "TrialData";

    // Start is called before the first frame update
    void Start()
    {
        //declare hardpoint
        string[] channels = new string[] {
            _streamName
        };
        new Hardpoint(channels, "localhost:6379");
    }

    private float? shoulderTarget;
    private float? elbowTarget;
    private float shoulderCurrent;
    private float elbowCurrent;

    // Update is called once per frame
    void Update()
    {
        var adjustments = new Dictionary<DOF, float>();
        var data = Hardpoint.ChannelDataDict;
        var reply = data[_streamName];
        foreach (var item in reply)
        {
            print($"key: {item.Key}: item {item.Value}");
            string jointKey = item.Key;
            try
            {
                var valueDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Value);
                float current = float.Parse(valueDict["target"]);
                float target = float.Parse(valueDict["current"]);
                
                if (jointKey.Equals("shoulderFlexion"))
                {
                    shoulderTarget = target;
                    shoulderCurrent = current;
                    adjustments.Add(DOF.ShoulderFlexion, shoulderCurrent);
                }
                else if (jointKey.Equals("elbowFlexion"))
                {
                    elbowTarget = target;
                    elbowCurrent = current;
                    adjustments.Add(DOF.ElbowFlexion, elbowCurrent);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing value: {ex}");
            }

        }
        _armController.AdjustAngles(adjustments);
        _armController.UpdateArm();
    }
}
