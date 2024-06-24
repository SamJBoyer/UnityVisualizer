using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using Unity.VisualScripting;
using System.Linq;

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
            string filePath = Path.Combine(Application.dataPath, "tasks");
            string jsonString = File.ReadAllText(filePath);
            _taskQueue = JsonConvert.DeserializeObject<Queue<Dictionary<DOF, float>>>(jsonString);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    private void Start(){
        LoadTasks();
        _taskArm = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _taskQueue.Count > 0)
        {
            if (_taskArm != null){
                Destroy(_taskArm);
            }
            Debug.Log("making new target");
            var taskVals = _taskQueue.Dequeue();
            _taskArm = CreateTargetArm(taskVals, _armObj, _masterArm.GetJointAngles());
        } else if (Input.GetKeyDown(KeyCode.J)){
            _taskQueue.Clear();
            LoadTasks();
        }
    }


    public GameObject CreateTargetArm(Dictionary<DOF, float> targetAngles, GameObject armObj, Dictionary<DOF, float> poseDict)
    {

        List<DOF> dofs = targetAngles.Keys.ToList();
        List<Focus> focuses = new List<Focus>();

        if (dofs.Contains(DOF.ShoulderAbduction) || dofs.Contains(DOF.ShoulderFlexion) || dofs.Contains(DOF.ShoulderRotation))
        {
            focuses.Add(Focus.Shoulder);
        }
        if (dofs.Contains(DOF.ElbowFlexion))
        {
            focuses.Add(Focus.Elbow);
        }
        if (dofs.Contains(DOF.WristAbduction) || dofs.Contains(DOF.WristFlexion) || dofs.Contains(DOF.WristSupination))
        {
            focuses.Add(Focus.Wrist);
        }

        GameObject newArm = GameObject.Instantiate(armObj);
        ArmController armController = newArm.GetComponent<ArmController>();
        armController.SetFieldsFromDict(poseDict);
        armController.AdjustAngles(targetAngles);
        armController.SetFocuses(focuses);
        armController.MakeTransparent();
        return newArm;
    }
}
