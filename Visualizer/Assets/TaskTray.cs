using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Linq;
using static ArmatureStructure;

public class TaskTray : MonoBehaviour
{

    private Queue<Dictionary<DOF, float>> _taskQueue = new Queue<Dictionary<DOF, float>>();
    [SerializeField] private ArmController _masterArm;
    [SerializeField] private GameObject _armObj;

    private GameObject _taskArm;

    private void LoadTasks()
    {
        try
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "tasks.json");
            string jsonString = File.ReadAllText(filePath);
            _taskQueue = JsonConvert.DeserializeObject<Queue<Dictionary<DOF, float>>>(jsonString);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    private void Start()
    {
        LoadTasks();
        _taskArm = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _taskQueue.Count > 0)
        {
            if (_taskArm != null)
            {
                Destroy(_taskArm);
            }
            Debug.Log("making new target");
            var taskVals = _taskQueue.Dequeue();
            //_taskArm = CreateTargetArm(taskVals, _armObj, _masterArm.GetCurrentArmature());
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            _taskQueue.Clear();
            LoadTasks();
        }
    }


    public GameObject CreateTargetArm(Dictionary<DOF, float> targetAngles, GameObject armObj, Dictionary<DOF, float> poseDict)
    {

        List<DOF> dofs = targetAngles.Keys.ToList();
        List<Focus> focuses = new List<Focus>();

        if (dofs.Contains(DOF.SHOULDERABDUCTION) || dofs.Contains(DOF.SHOULDERFLEXION) || dofs.Contains(DOF.SHOULDERROTATION))
        {
            focuses.Add(Focus.Shoulder);
        }
        if (dofs.Contains(DOF.ELBOWFLEXION))
        {
            focuses.Add(Focus.Elbow);
        }
        if (dofs.Contains(DOF.WRISTABDUCTION) || dofs.Contains(DOF.WRISTFLEXION) || dofs.Contains(DOF.WRISTSUPINATION))
        {
            focuses.Add(Focus.Wrist);
        }

        GameObject newArm = GameObject.Instantiate(armObj);
        ArmController armController = newArm.GetComponent<ArmController>();
        //armController.SetFieldsFromDict(poseDict);
        armController.AdjustAngles(targetAngles);
        armController.SetFocuses(focuses);
        armController.MakeTransparent();
        return newArm;
    }
}
