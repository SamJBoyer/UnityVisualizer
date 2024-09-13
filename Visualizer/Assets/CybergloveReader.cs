using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using static ArmatureStructure;

public class CybergloveReader : MonoBehaviour
{
    [SerializeField] private ArmController _armController;
    private float[][][] _cybergloveTrialData;
    private int i;
    private float[][] _trial1;

    void Start()
    {
        // Load and deserialize the JSON file
        string filePath = Path.Combine(Application.dataPath, "cyberglovedata.json"); // Replace with your JSON file path
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            _cybergloveTrialData = JsonConvert.DeserializeObject<float[][][]>(json);
            Debug.Log("JSON file loaded and deserialized successfully.");
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
        _trial1 = _cybergloveTrialData[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("input");
            i++;

        }

        var trialData = _trial1[i];
        var values = GetValuesFromData(trialData);
        _armController.AdjustAngles(values);
    }

    private Dictionary<DOF, float> GetValuesFromData(float[] input)
    {
        return new Dictionary<DOF, float> {
            {DOF.INDEX1, input[4]},
            {DOF.INDEX2, input[5]},
            {DOF.INDEX3, input[6]},

            {DOF.MIDDLE1, input[8]},
            {DOF.MIDDLE2, input[9]},
            {DOF.MIDDLE3, input[10]},

            {DOF.RING1, input[12]},
            {DOF.RING2, input[13]},
            {DOF.RING3, input[14]},

            {DOF.PINKY1, input[16]},
            {DOF.PINKY2, input[17]},
            {DOF.PINKY3, input[18]},
        };
    }
}
